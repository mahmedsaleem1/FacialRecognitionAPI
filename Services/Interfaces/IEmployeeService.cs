using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;

namespace FacialRecognitionAPI.Services.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeResponse> OnboardAsync(OnboardEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeResponse?> GetByCodeAsync(string employeeCode, CancellationToken cancellationToken = default);
    Task<List<EmployeeResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResponse> UpdateFaceAsync(Guid id, UpdateEmployeeFaceRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
