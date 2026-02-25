namespace FacialRecognitionAPI.Services.Interfaces;

/// <summary>
/// Manages image uploads and deletions on Cloudinary.
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload a face image to Cloudinary.
    /// </summary>
    /// <param name="imageData">Raw image bytes (JPEG/PNG).</param>
    /// <param name="fileName">Desired file name (without extension).</param>
    /// <returns>(SecureUrl, PublicId)</returns>
    Task<(string Url, string PublicId)> UploadImageAsync(byte[] imageData, string fileName);

    /// <summary>
    /// Delete an image from Cloudinary by public ID.
    /// </summary>
    Task<bool> DeleteImageAsync(string publicId);
}
