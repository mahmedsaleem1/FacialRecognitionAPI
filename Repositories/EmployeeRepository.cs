using FacialRecognitionAPI.Data;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacialRecognitionAPI.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Employee?> GetByCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode, cancellationToken);

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(e => e.Email == email, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(e => e.Email == email, cancellationToken);

    public async Task<List<Employee>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync(cancellationToken);

    public async Task<List<Employee>> GetAllActiveWithEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _dbSet
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);

        // Filter in memory — EF Core cannot translate byte[].Length to SQL
        return employees.Where(e => e.EncryptedEmbedding.Length > 0).ToList();
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(e => e.IsActive, cancellationToken);

    public async Task<int> CountActiveByDepartmentAsync(string department, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(e => e.IsActive && e.Department == department, cancellationToken);

    public async Task<List<string>> GetDistinctDepartmentsAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(e => e.IsActive && e.Department != null)
            .Select(e => e.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(cancellationToken);
}
