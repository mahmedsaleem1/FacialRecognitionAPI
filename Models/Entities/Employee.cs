namespace FacialRecognitionAPI.Models.Entities;

/// <summary>
/// Represents an internee/employee in the attendance system.
/// </summary>
public class Employee
{
    public Guid Id { get; set; }

    /// <summary>
    /// Auto-generated unique identifier displayed to users (e.g., "INT-20260225-A1B2").
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }

    /// <summary>
    /// URL of the reference face image stored in Cloudinary.
    /// </summary>
    public string CloudinaryImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Cloudinary public ID for managing the uploaded image.
    /// </summary>
    public string CloudinaryPublicId { get; set; } = string.Empty;

    /// <summary>
    /// AES-256-GCM encrypted face embedding vector extracted from the reference image.
    /// </summary>
    public byte[] EncryptedEmbedding { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// AES nonce/IV used for encryption (unique per record).
    /// </summary>
    public byte[] EncryptionIv { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// AES-GCM authentication tag for integrity verification.
    /// </summary>
    public byte[] EncryptionTag { get; set; } = Array.Empty<byte>();

    public bool IsActive { get; set; } = true;
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
