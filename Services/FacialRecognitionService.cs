using System.Numerics;
using System.Security.Cryptography;
using FacialRecognitionAPI.Configuration;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FacialRecognitionAPI.Services;

/// <summary>
/// Facial recognition service using ONNX Runtime for face detection and embedding extraction.
/// 
/// Models required:
/// - Face Detection: Ultra-Light-Fast-Generic-Face-Detector (version-RFB-640.onnx)
///   Download: https://github.com/onnx/models/tree/main/validated/vision/body_analysis/ultraface
/// - Face Recognition: ArcFace ResNet100 (arcfaceresnet100-11.onnx)
///   Download: https://github.com/onnx/models/tree/main/validated/vision/body_analysis/arcface
///
/// In development mode (UseDevelopmentMode=true), a hash-based pseudo-embedding is generated
/// for testing without ONNX models.
/// </summary>
public class FacialRecognitionService : IFacialRecognitionService, IDisposable
{
    private readonly FaceRecognitionSettings _settings;
    private readonly ILogger<FacialRecognitionService> _logger;
    private readonly InferenceSession? _detectionSession;
    private readonly InferenceSession? _recognitionSession;
    private readonly bool _modelsAvailable;

    public FacialRecognitionService(
        IOptions<FaceRecognitionSettings> settings,
        ILogger<FacialRecognitionService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (_settings.UseDevelopmentMode)
        {
            _logger.LogWarning("FacialRecognitionService running in DEVELOPMENT MODE. " +
                "Using hash-based pseudo-embeddings. Do NOT use in production.");
            _modelsAvailable = false;
            return;
        }

        // Load ONNX models
        try
        {
            var detectionPath = Path.Combine(AppContext.BaseDirectory, _settings.DetectionModelPath);
            var recognitionPath = Path.Combine(AppContext.BaseDirectory, _settings.RecognitionModelPath);

            if (!File.Exists(detectionPath))
            {
                _logger.LogError("Face detection model not found at: {Path}. " +
                    "Download from https://github.com/onnx/models/tree/main/validated/vision/body_analysis/ultraface", detectionPath);
                _modelsAvailable = false;
                return;
            }

            if (!File.Exists(recognitionPath))
            {
                _logger.LogError("Face recognition model not found at: {Path}. " +
                    "Download from https://github.com/onnx/models/tree/main/validated/vision/body_analysis/arcface", recognitionPath);
                _modelsAvailable = false;
                return;
            }

            var sessionOptions = new Microsoft.ML.OnnxRuntime.SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
                ExecutionMode = ExecutionMode.ORT_SEQUENTIAL
            };

            _detectionSession = new InferenceSession(detectionPath, sessionOptions);
            _recognitionSession = new InferenceSession(recognitionPath, sessionOptions);
            _modelsAvailable = true;

            _logger.LogInformation("ONNX face models loaded successfully.");
            _logger.LogInformation("Detection model inputs: {Inputs}",
                string.Join(", ", _detectionSession.InputMetadata.Select(x => $"{x.Key}: [{string.Join(",", x.Value.Dimensions)}]")));
            _logger.LogInformation("Recognition model inputs: {Inputs}",
                string.Join(", ", _recognitionSession.InputMetadata.Select(x => $"{x.Key}: [{string.Join(",", x.Value.Dimensions)}]")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load ONNX models. Falling back to development mode.");
            _modelsAvailable = false;
        }
    }

    public async Task<float[]?> ExtractFaceEmbeddingAsync(byte[] imageData)
    {
        if (!_modelsAvailable)
            return await ExtractDevelopmentEmbeddingAsync(imageData);

        return await Task.Run(() => ExtractEmbeddingWithOnnx(imageData));
    }

    public float ComputeSimilarity(float[] embedding1, float[] embedding2)
    {
        if (embedding1.Length != embedding2.Length)
            throw new ArgumentException("Embeddings must have the same dimension.");

        // Cosine similarity using SIMD-optimized operations
        float dotProduct = 0f, norm1 = 0f, norm2 = 0f;

        int simdLength = System.Numerics.Vector<float>.Count;
        int i = 0;

        // SIMD-vectorized computation
        for (; i <= embedding1.Length - simdLength; i += simdLength)
        {
            var v1 = new System.Numerics.Vector<float>(embedding1, i);
            var v2 = new System.Numerics.Vector<float>(embedding2, i);

            dotProduct += System.Numerics.Vector.Dot(v1, v2);
            norm1 += System.Numerics.Vector.Dot(v1, v1);
            norm2 += System.Numerics.Vector.Dot(v2, v2);
        }

        // Remaining elements
        for (; i < embedding1.Length; i++)
        {
            dotProduct += embedding1[i] * embedding2[i];
            norm1 += embedding1[i] * embedding1[i];
            norm2 += embedding2[i] * embedding2[i];
        }

        var denominator = MathF.Sqrt(norm1) * MathF.Sqrt(norm2);
        return denominator == 0 ? 0f : dotProduct / denominator;
    }

    public bool VerifyFace(float[] embedding1, float[] embedding2)
    {
        var similarity = ComputeSimilarity(embedding1, embedding2);

        _logger.LogInformation("Face verification: similarity={Similarity:F4}, threshold={Threshold:F4}, match={Match}",
            similarity, _settings.VerificationThreshold, similarity >= _settings.VerificationThreshold);

        return similarity >= _settings.VerificationThreshold;
    }

    public string ComputeImageHash(byte[] imageData)
    {
        var hash = SHA256.HashData(imageData);
        return Convert.ToHexStringLower(hash);
    }

    #region ONNX Model Inference

    private float[]? ExtractEmbeddingWithOnnx(byte[] imageData)
    {
        try
        {
            using var image = Image.Load<Rgb24>(imageData);

            // Step 1: Detect face
            var faceRegion = DetectFace(image);
            if (faceRegion == null)
            {
                _logger.LogWarning("No face detected in the image.");
                return null;
            }

            // Step 2: Crop and prepare face for recognition
            var (x, y, w, h) = faceRegion.Value;

            // Add margin around detected face (20%)
            int margin = (int)(Math.Max(w, h) * 0.2f);
            int cropX = Math.Max(0, x - margin);
            int cropY = Math.Max(0, y - margin);
            int cropW = Math.Min(image.Width - cropX, w + 2 * margin);
            int cropH = Math.Min(image.Height - cropY, h + 2 * margin);

            using var faceImage = image.Clone(ctx =>
            {
                ctx.Crop(new Rectangle(cropX, cropY, cropW, cropH));
                ctx.Resize(_settings.RecognitionInputSize, _settings.RecognitionInputSize);
            });

            // Step 3: Extract embedding
            var embedding = ExtractRecognitionEmbedding(faceImage);

            // L2 normalize the embedding
            L2Normalize(embedding);

            _logger.LogInformation("Face embedding extracted successfully ({Dimensions} dimensions).", embedding.Length);
            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting face embedding with ONNX models.");
            return null;
        }
    }

    private (int X, int Y, int Width, int Height)? DetectFace(Image<Rgb24> image)
    {
        int inputW = _settings.DetectionInputWidth;
        int inputH = _settings.DetectionInputHeight;

        // Preprocess: resize and normalize to [0,1], NCHW format
        using var resized = image.Clone(ctx => ctx.Resize(inputW, inputH));
        var inputTensor = new DenseTensor<float>(new[] { 1, 3, inputH, inputW });

        resized.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < inputH; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < inputW; x++)
                {
                    var pixel = row[x];
                    inputTensor[0, 0, y, x] = (pixel.R - 127f) / 128f;
                    inputTensor[0, 1, y, x] = (pixel.G - 127f) / 128f;
                    inputTensor[0, 2, y, x] = (pixel.B - 127f) / 128f;
                }
            }
        });

        // Run detection model
        var inputName = _detectionSession!.InputMetadata.Keys.First();
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        };

        using var results = _detectionSession.Run(inputs);
        var outputNames = _detectionSession.OutputMetadata.Keys.ToList();

        // UltraFace outputs: "scores" and "boxes"
        var scores = results.First(r => r.Name.Contains("score") || r.Name == outputNames[0])
            .AsTensor<float>();
        var boxes = results.First(r => r.Name.Contains("box") || r.Name == outputNames[1])
            .AsTensor<float>();

        // Find best detection above threshold
        float bestScore = 0f;
        int bestIdx = -1;

        var numDetections = scores.Dimensions[1];
        for (int i = 0; i < numDetections; i++)
        {
            // Score for "face" class (index 1)
            float score = scores.Dimensions.Length == 3 ? scores[0, i, 1] : scores[0, i];
            if (score > _settings.DetectionConfidenceThreshold && score > bestScore)
            {
                bestScore = score;
                bestIdx = i;
            }
        }

        if (bestIdx < 0)
            return null;

        // Convert normalized coordinates to pixel coordinates
        float bx1, by1, bx2, by2;
        if (boxes.Dimensions.Length == 3)
        {
            bx1 = boxes[0, bestIdx, 0];
            by1 = boxes[0, bestIdx, 1];
            bx2 = boxes[0, bestIdx, 2];
            by2 = boxes[0, bestIdx, 3];
        }
        else
        {
            bx1 = boxes[0, bestIdx * 4 + 0];
            by1 = boxes[0, bestIdx * 4 + 1];
            bx2 = boxes[0, bestIdx * 4 + 2];
            by2 = boxes[0, bestIdx * 4 + 3];
        }

        int fx = (int)(bx1 * image.Width);
        int fy = (int)(by1 * image.Height);
        int fw = (int)((bx2 - bx1) * image.Width);
        int fh = (int)((by2 - by1) * image.Height);

        _logger.LogDebug("Face detected at ({X},{Y}) size {W}x{H}, confidence={Score:F3}",
            fx, fy, fw, fh, bestScore);

        return (fx, fy, fw, fh);
    }

    private float[] ExtractRecognitionEmbedding(Image<Rgb24> faceImage)
    {
        int size = _settings.RecognitionInputSize;
        var inputTensor = new DenseTensor<float>(new[] { 1, 3, size, size });

        faceImage.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < size; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < size; x++)
                {
                    var pixel = row[x];
                    // ArcFace normalization: (pixel - 127.5) / 127.5
                    inputTensor[0, 0, y, x] = (pixel.R - 127.5f) / 127.5f;
                    inputTensor[0, 1, y, x] = (pixel.G - 127.5f) / 127.5f;
                    inputTensor[0, 2, y, x] = (pixel.B - 127.5f) / 127.5f;
                }
            }
        });

        var inputName = _recognitionSession!.InputMetadata.Keys.First();
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        };

        using var results = _recognitionSession.Run(inputs);
        var output = results.First().AsTensor<float>();

        // Extract embedding vector
        var embeddingSize = output.Dimensions[1];
        var embedding = new float[embeddingSize];
        for (int i = 0; i < embeddingSize; i++)
            embedding[i] = output[0, i];

        return embedding;
    }

    private static void L2Normalize(float[] vector)
    {
        float norm = 0f;
        for (int i = 0; i < vector.Length; i++)
            norm += vector[i] * vector[i];

        norm = MathF.Sqrt(norm);
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
                vector[i] /= norm;
        }
    }

    #endregion

    #region Development Mode (Hash-based Pseudo-Embeddings)

    /// <summary>
    /// Generates a deterministic pseudo-embedding from image data for development/testing.
    /// The same image always produces the same embedding, allowing testing of the full pipeline
    /// without actual ONNX models.
    /// </summary>
    private Task<float[]?> ExtractDevelopmentEmbeddingAsync(byte[] imageData)
    {
        _logger.LogWarning("Using development mode pseudo-embedding. NOT suitable for production.");

        try
        {
            // Validate image can be loaded
            using var image = Image.Load<Rgb24>(imageData);

            // Generate deterministic 512-dim embedding from image hash
            var hash = SHA256.HashData(imageData);
            var embedding = new float[512];

            // Use hash bytes as seed to generate a full 512-dim vector
            var rng = new Random(BitConverter.ToInt32(hash, 0));
            for (int i = 0; i < 512; i++)
                embedding[i] = (float)(rng.NextDouble() * 2.0 - 1.0);

            // L2 normalize
            L2Normalize(embedding);

            return Task.FromResult<float[]?>(embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process image in development mode.");
            return Task.FromResult<float[]?>(null);
        }
    }

    #endregion

    public void Dispose()
    {
        _detectionSession?.Dispose();
        _recognitionSession?.Dispose();
        GC.SuppressFinalize(this);
    }
}
