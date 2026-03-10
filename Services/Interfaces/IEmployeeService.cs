using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;

namespace FacialRecognitionAPI.Services.Interfaces;

public interface IEmployeeService
{
    Task<RegisterEmployeeResponse> RegisterAsync(RegisterEmployeeFormRequest form, DateOnly joinDate, CancellationToken cancellationToken = default);
}
