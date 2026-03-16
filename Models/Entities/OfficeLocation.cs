namespace FacialRecognitionAPI.Models.Entities;

public class OfficeLocation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Main Office";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int AllowedRadiusMeters { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
