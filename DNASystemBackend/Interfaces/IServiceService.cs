
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(string id);
        Task<Service> CreateAsync(Service model);
        Task<bool> UpdateAsync(string id, Service model);
        Task<bool> DeleteAsync(string id);
        Task<List<string>> GetCategoriesAsync();
    }
}
