namespace FacialRecognitionAPI.Services.Interfaces;

/// <summary>
/// Facial recognition service using ONNX models for face detection and embedding extraction.
/// </summary>
public interface IFacialRecognitionService
{
    /// <summary>
    /// Extracts a 512-dimensional face embedding from an image.
    /// </summary>
    /// <param name="imageData">Raw image bytes (JPEG/PNG).</param>
    /// <returns>512-dimensional float array embedding, or null if no face detected.</returns>
    Task<float[]?> ExtractFaceEmbeddingAsync(byte[] imageData);

    /// <summary>
    /// Computes cosine similarity between two face embeddings.
    /// </summary>
    /// <returns>Similarity score between -1.0 and 1.0.</returns>
    float ComputeSimilarity(float[] embedding1, float[] embedding2);

    /// <summary>
    /// Verifies if two face embeddings belong to the same person.
    /// </summary>
    bool VerifyFace(float[] embedding1, float[] embedding2);

    /// <summary>
    /// Computes SHA-256 hash of image data for deduplication.
    /// </summary>
    string ComputeImageHash(byte[] imageData);
}
