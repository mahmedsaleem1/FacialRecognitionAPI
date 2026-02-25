namespace FacialRecognitionAPI.Configuration;

public class FaceRecognitionSettings
{
    public const string SectionName = "FaceRecognition";

    /// <summary>
    /// Path to the face detection ONNX model (e.g., version-RFB-640.onnx).
    /// </summary>
    public string DetectionModelPath { get; set; } = "OnnxModels/face_detection.onnx";

    /// <summary>
    /// Path to the face recognition/embedding ONNX model (e.g., arcfaceresnet100-11.onnx).
    /// </summary>
    public string RecognitionModelPath { get; set; } = "OnnxModels/face_recognition.onnx";

    /// <summary>
    /// Cosine similarity threshold for face verification (0.0 - 1.0).
    /// Higher = stricter matching; typical range 0.4 - 0.6.
    /// </summary>
    public float VerificationThreshold { get; set; } = 0.45f;

    /// <summary>
    /// Face detection confidence threshold.
    /// </summary>
    public float DetectionConfidenceThreshold { get; set; } = 0.7f;

    /// <summary>
    /// Width expected by the detection model input.
    /// </summary>
    public int DetectionInputWidth { get; set; } = 640;

    /// <summary>
    /// Height expected by the detection model input.
    /// </summary>
    public int DetectionInputHeight { get; set; } = 480;

    /// <summary>
    /// Size expected by the recognition model (square input).
    /// </summary>
    public int RecognitionInputSize { get; set; } = 112;

    /// <summary>
    /// Enable development mode (uses hash-based pseudo-embeddings when ONNX models are unavailable).
    /// </summary>
    public bool UseDevelopmentMode { get; set; } = false;
}
