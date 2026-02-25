using FacialRecognitionAPI.Models.Entities;

namespace FacialRecognitionAPI.Repositories.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByCodeAsync(string employeeCode, CancellationToken cancellationToken = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Employee>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<List<Employee>> GetAllActiveWithEmbeddingsAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctDepartmentsAsync(CancellationToken cancellationToken = default);
}
