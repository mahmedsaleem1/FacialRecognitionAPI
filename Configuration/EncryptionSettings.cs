namespace FacialRecognitionAPI.Configuration;

public class EncryptionSettings
{
    public const string SectionName = "EncryptionSettings";

    /// <summary>
    /// Base64-encoded 256-bit (32-byte) AES key for encrypting face embeddings.
    /// Generate with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
    /// </summary>
    public string AesKey { get; set; } = string.Empty;
}
