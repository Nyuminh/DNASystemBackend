
using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(string id);
        Task<(bool success, string? message)> CreateAsync(ServiceDto model);
        Task<(bool success, string? message)> UpdateAsync(string id, UpdateServiceDto model);
        Task<(bool success, string? message)> DeleteAsync(string id);

        Task<(bool success, string? message)> DeleteWithCascadeAsync(string id);
        Task<List<string>> GetCategoriesAsync();
    }
}