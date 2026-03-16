using FacialRecognitionAPI.Data;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacialRecognitionAPI.Repositories;

public class OfficeLocationRepository : Repository<OfficeLocation>, IOfficeLocationRepository
{
    public OfficeLocationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<OfficeLocation?> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);
}
