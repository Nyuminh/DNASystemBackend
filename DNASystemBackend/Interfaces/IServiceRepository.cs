using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(string id);
        Task<Service> CreateAsync(Service service);
        Task<bool> UpdateAsync(string id, Service service);
        Task<bool> DeleteAsync(string id);
        Task<List<string>> GetCategoriesAsync();
        Task<string> GenerateServiceIdAsync();
    }
}