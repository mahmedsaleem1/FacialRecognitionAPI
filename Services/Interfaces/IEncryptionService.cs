namespace FacialRecognitionAPI.Services.Interfaces;

/// <summary>
/// AES-256-GCM encryption for securing face embedding data at rest.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a float array (face embedding) using AES-256-GCM.
    /// Returns (ciphertext, iv, tag).
    /// </summary>
    (byte[] CipherText, byte[] Iv, byte[] Tag) Encrypt(float[] embedding);

    /// <summary>
    /// Decrypts an AES-256-GCM encrypted face embedding back to float array.
    /// </summary>
    float[] Decrypt(byte[] cipherText, byte[] iv, byte[] tag);
}
