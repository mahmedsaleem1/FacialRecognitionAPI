using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;

namespace FacialRecognitionAPI.Services.Interfaces;

public interface IAttendanceService
{
    Task<MarkAttendanceResponse> MarkAttendanceAsync(MarkAttendanceRequest request, DateTimeOffset checkInTimestamp, CancellationToken cancellationToken = default);
    Task<List<DailyAttendanceRecord>> GetDailyAttendanceAsync(DateOnly date, CancellationToken cancellationToken = default);
}
