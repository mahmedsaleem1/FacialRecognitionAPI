using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FacialRecognitionAPI.Configuration;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace FacialRecognitionAPI.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<(string Url, string PublicId)> UploadImageAsync(byte[] imageData, string fileName)
    {
        await using var stream = new MemoryStream(imageData);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = _settings.UploadFolder,
            PublicId = fileName,
            Overwrite = true,
            Transformation = new Transformation()
                .Width(500).Height(500).Crop("fill").Gravity("face")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {PublicId} -> {Url}", result.PublicId, result.SecureUrl);
        return (result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        if (result.Result == "ok")
        {
            _logger.LogInformation("Cloudinary image deleted: {PublicId}", publicId);
            return true;
        }

        _logger.LogWarning("Cloudinary delete failed for {PublicId}: {Result}", publicId, result.Result);
        return false;
    }
}
