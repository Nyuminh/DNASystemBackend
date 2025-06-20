using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(string id);
        Task<Booking> CreateAsync(Booking booking);
        Task<bool> UpdateAsync(string id, Booking updated);
        Task<bool> DeleteAsync(string id);
        Task<List<DateTime>> GetAvailableSchedulesAsync();
    }
}
