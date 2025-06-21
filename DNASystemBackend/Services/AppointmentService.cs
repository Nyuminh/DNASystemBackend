using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;

namespace DNASystemBackend.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;

        public AppointmentService(IAppointmentRepository repository)
        {
            _repository = repository;
        }        public Task<IEnumerable<Booking>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<Booking?> GetByIdAsync(string id)
            => _repository.GetByIdAsync(id);        public async Task<Booking> CreateAsync(Booking booking)
        {
            try
            {
                // Generate a new booking ID if not provided
                if (string.IsNullOrEmpty(booking.BookingId))
                {
                    booking.BookingId = await _repository.GenerateBookingIdAsync();
                }

                await _repository.CreateAsync(booking);
                return booking;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo đặt lịch: {ex.Message}", ex);
            }
        }

        public Task<bool> UpdateAsync(string id, Booking updated)
            => _repository.UpdateAsync(id, updated);

        public Task<bool> DeleteAsync(string id)
            => _repository.DeleteAsync(id);

        public Task<List<DateTime>> GetAvailableSchedulesAsync()
            => _repository.GetAvailableSchedulesAsync();
    }
}
