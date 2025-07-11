using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IKitRepository
    {
        Task<IEnumerable<Kit>> GetAllAsync();
        Task<Kit?> GetByIdAsync(string id);
        Task<Kit> CreateAsync(Kit kit);
        Task<bool> UpdateStatusAsync(string id, Kit status);
        Task<IEnumerable<Kit>> GetTrackingSamplesAsync();
        Task<IEnumerable<Kit>> GetCollectionSamplesAsync();
        Task<string> GenerateKitIdAsync();
    }
}
