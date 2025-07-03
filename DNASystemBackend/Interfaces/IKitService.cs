using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IKitService
    {
        Task<IEnumerable<Kit>> GetAllAsync();
        Task<Kit?> GetByIdAsync(string id);
        Task<(bool success, string? message)> CreateAsync(CreateKitDto kit);
        Task<(bool success, string? message)> UpdateStatusAsync(string id, UpdateKitDto status);
        Task<(bool success, string? message)> DeleteAsync(string id);
        Task<IEnumerable<Kit>> GetTrackingSamplesAsync();
        Task<IEnumerable<Kit>> GetCollectionSamplesAsync();
        Task<Kit?> GetByBookingIdAsync(string bookingId);
    }
}
