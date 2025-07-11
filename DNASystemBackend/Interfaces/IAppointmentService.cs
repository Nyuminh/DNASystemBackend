using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<IEnumerable<Booking>> GetByServiceIdAsync(string serviceId);
        Task<Booking?> GetByIdAsync(string id);
        Task<(bool success, string? message, Booking? booking)> CreateAsync(AppointmentDto dto);
        Task<(bool success, string? message)> UpdateAsync(string id, UpdateAppointDto updated);
        Task<(bool success, string? message)> DeleteAsync(string id);
        Task<List<DateTime>> GetAvailableSchedulesAsync();
        
    }
}