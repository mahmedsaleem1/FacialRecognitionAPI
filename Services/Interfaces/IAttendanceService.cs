using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;

namespace FacialRecognitionAPI.Services.Interfaces;

public interface IAttendanceService
{
    Task<MarkAttendanceResponse> MarkCheckInAsync(MarkAttendanceRequest request, CancellationToken cancellationToken = default);
    Task<MarkAttendanceResponse> MarkCheckOutAsync(CheckOutRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<AttendanceResponse>> GetAttendanceRecordsAsync(AttendanceQueryParams query, CancellationToken cancellationToken = default);
    Task<List<AttendanceResponse>> GetTodayAttendanceAsync(CancellationToken cancellationToken = default);
    Task<AttendanceResponse?> GetEmployeeTodayAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
