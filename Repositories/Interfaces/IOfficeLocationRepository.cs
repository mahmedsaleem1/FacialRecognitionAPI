using FacialRecognitionAPI.Models.Entities;

namespace FacialRecognitionAPI.Repositories.Interfaces;

public interface IOfficeLocationRepository : IRepository<OfficeLocation>
{
    Task<OfficeLocation?> GetActiveAsync(CancellationToken cancellationToken = default);
}
