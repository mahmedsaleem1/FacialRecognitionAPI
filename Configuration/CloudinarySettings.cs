namespace FacialRecognitionAPI.Configuration;

public class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Cloudinary folder to organize uploaded employee face images.
    /// </summary>
    public string UploadFolder { get; set; } = "facial-attendance/employees";
}
