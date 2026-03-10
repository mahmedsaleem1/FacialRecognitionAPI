namespace FacialRecognitionAPI.Models.DTOs.Responses;

// Used by GlobalExceptionHandlerMiddleware to return { "message": "..." } on errors.
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;

    public static ErrorResponse From(string message) => new() { Message = message };
}
