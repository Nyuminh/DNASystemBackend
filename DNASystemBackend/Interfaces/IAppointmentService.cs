using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(string id);
        Task<(bool success, string? message)> CreateAsync(AppointmentDto dto);
        Task<(bool success, string? message)> UpdateAsync(string id, UpdateAppointDto updated);
        Task<(bool success, string? message)> DeleteAsync(string id);
        Task<List<DateTime>> GetAvailableSchedulesAsync();
        Task<Booking?> GetByServiceIdAsync(string serviceId);
    }
}