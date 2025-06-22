using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(string id);
        Task CreateAsync(Booking booking);
        Task<bool> UpdateAsync(string id, Booking updated);
        Task<bool> DeleteAsync(string id);
        Task<List<DateTime>> GetAvailableSchedulesAsync();
        Task<string> GenerateBookingIdAsync();
        Task SaveAsync();
    }
}