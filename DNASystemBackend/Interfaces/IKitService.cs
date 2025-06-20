using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IKitService
    {
        Task<IEnumerable<Kit>> GetAllAsync();
        Task<Kit?> GetByIdAsync(string id);
        Task<Kit> CreateAsync(Kit kit);
        Task<bool> UpdateStatusAsync(string id, string status);
        Task<IEnumerable<Kit>> GetTrackingSamplesAsync();
        Task<IEnumerable<Kit>> GetCollectionSamplesAsync();
    }
}
