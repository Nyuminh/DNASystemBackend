using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

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


        public Task<IEnumerable<Booking>> GetByServiceIdAsync(string serviceID)
            => _repository.GetByServiceIdAsync(serviceID);
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
                    Date = dto.Date,
                    Address = dto.Address,
                    Method = dto.Method,
                    Status =dto.Status
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
                Console.WriteLine($"Creating booking: ID={booking.BookingId}, Customer={booking.CustomerId}, Staff={booking.StaffId}");
                await _repository.CreateAsync(booking);
                Console.WriteLine("Repository CreateAsync completed");
                return (true, "Tạo lịch hẹn thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating booking: {ex}");
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
            if (updated.Address != null)
                booking.Address = updated.Address;

            if (updated.Method != null)
                booking.Method = updated.Method;
            booking.Status = updated.Status;

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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _repository.GetByIdAsync(id);
                if (booking == null)
                    return (false, "Không tìm thấy lịch hẹn.");

                // Get related invoices
                var invoices = await _context.Invoices
                    .Where(i => i.BookingId == id)
                    .ToListAsync();

                // For each invoice, delete its invoice details first
                foreach (var invoice in invoices)
                {
                    var invoiceDetails = await _context.InvoiceDetails
                        .Where(d => d.InvoiceId == invoice.InvoiceId)
                        .ToListAsync();

                    if (invoiceDetails.Any())
                    {
                        _context.InvoiceDetails.RemoveRange(invoiceDetails);
                    }
                }

                // Now delete all the invoices
                if (invoices.Any())
                {
                    _context.Invoices.RemoveRange(invoices);
                }

                // Finally delete the booking
                _context.Bookings.Remove(booking);

                // Save all changes
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return (true, "Xóa lịch hẹn và tất cả dữ liệu liên quan thành công.");
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync();
                return (false, $"Lỗi khi xóa lịch hẹn: {ex.Message}");
            }
        }


        public Task<List<DateTime>> GetAvailableSchedulesAsync()
            => _repository.GetAvailableSchedulesAsync();
    }
}