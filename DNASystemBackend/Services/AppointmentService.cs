using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;

namespace DNASystemBackend.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        private readonly DnasystemContext _context;

        public AppointmentService(IAppointmentRepository repository, DnasystemContext context)
        {
            _repository = repository;
            _context = context;
        }
        public Task<IEnumerable<Booking>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<Booking?> GetByIdAsync(string id)
            => _repository.GetByIdAsync(id);

        public async Task<(bool success, string? message)> CreateAsync(AppointmentDto dto)
        {
            try
            {
                var booking = new Booking
                {
                    CustomerId = dto.CustomerId,
                    StaffId = dto.StaffId,
                    ServiceId = dto.ServiceId,
                    Date = dto.Date
                };

                // Generate a new booking ID if not provided
                if (string.IsNullOrEmpty(dto.BookingId))
                {
                    booking.BookingId = await _repository.GenerateBookingIdAsync();
                }
                else
                {
                    booking.BookingId = dto.BookingId;
                }

                await _repository.CreateAsync(booking);
                return (true, "Tạo lịch hẹn thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo đặt lịch: {ex.Message}");
            }
        }

        public async Task<(bool success, string? message)> UpdateAsync(string id, UpdateAppointDto updated)
        {
            var booking = await _repository.GetByIdAsync(id);
            if (booking == null) return (false, "Không tìm thấy lịch hẹn.");

            // Update properties from UpdateAppointDto

            booking.StaffId = updated.StaffId;
            booking.ServiceId = updated.ServiceId;
            booking.Date = updated.Date;

            try
            {
                await _repository.UpdateAsync(id, booking);
                return (true, "Cập nhật lịch hẹn thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật lịch hẹn: {ex.Message}");
            }
        }
        public async Task<(bool success, string? message)> DeleteAsync(string id)
        {
            var booking = await _repository.GetByIdAsync(id);
            if (booking == null) return (false, "Không tìm thấy lịch hẹn.");

            try
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return (true, "Xóa lịch hẹn thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa lịch hẹn: {ex.Message}");
            }
        }

        public Task<List<DateTime>> GetAvailableSchedulesAsync()
            => _repository.GetAvailableSchedulesAsync();
    }
}