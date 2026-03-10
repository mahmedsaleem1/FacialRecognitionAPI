using FacialRecognitionAPI.Models.Entities;

namespace FacialRecognitionAPI.Repositories.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
