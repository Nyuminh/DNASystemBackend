using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IRelativeRepository
    {
        Task<IEnumerable<Relative>> GetAllAsync();
        Task<Relative?> GetByIdAsync(string id);
        Task<Relative?> GetByUserIdAsync(string userId);
        Task<Relative> CreateAsync(Relative relative);
        Task<bool> UpdateAsync(Relative relative);
        Task<bool> DeleteAsync(string id);
        Task<Relative?> GetByBookingIdAsync(string bookingId);
    }
}
