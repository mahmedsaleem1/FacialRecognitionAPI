using System.Security.Cryptography;
using FacialRecognitionAPI.Configuration;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace FacialRecognitionAPI.Services;

/// <summary>
/// AES-256-GCM encryption for face embedding data at rest.
/// Each encryption produces a unique nonce + authentication tag.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IOptions<EncryptionSettings> settings, ILogger<EncryptionService> logger)
    {
        _logger = logger;

        var keyBase64 = settings.Value.AesKey;
        if (string.IsNullOrWhiteSpace(keyBase64))
            throw new InvalidOperationException("AES encryption key is not configured. Set EncryptionSettings:AesKey in appsettings.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException("AES key must be exactly 32 bytes (256-bit). Provide a valid Base64-encoded 32-byte key.");
    }

    public (byte[] CipherText, byte[] Iv, byte[] Tag) Encrypt(float[] embedding)
    {
        // Convert float[] to byte[]
        var plainBytes = new byte[embedding.Length * sizeof(float)];
        Buffer.BlockCopy(embedding, 0, plainBytes, 0, plainBytes.Length);

        // AES-GCM: 12-byte nonce (recommended), 16-byte tag
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var cipherText = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipherText, tag);

        _logger.LogDebug("Encrypted face embedding: {EmbeddingLength} floats -> {CipherLength} bytes",
            embedding.Length, cipherText.Length);

        return (cipherText, nonce, tag);
    }

    public float[] Decrypt(byte[] cipherText, byte[] iv, byte[] tag)
    {
        var plainBytes = new byte[cipherText.Length];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(iv, cipherText, tag, plainBytes);

        // Convert byte[] back to float[]
        var embedding = new float[plainBytes.Length / sizeof(float)];
        Buffer.BlockCopy(plainBytes, 0, embedding, 0, plainBytes.Length);

        _logger.LogDebug("Decrypted face embedding: {CipherLength} bytes -> {EmbeddingLength} floats",
            cipherText.Length, embedding.Length);

        return embedding;
    }
}
