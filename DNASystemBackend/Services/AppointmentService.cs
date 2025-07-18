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

        public async Task<(bool success, string? message, Booking? booking)> CreateAsync(AppointmentDto dto)
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
                return (true, "Tạo lịch hẹn thành công.", booking);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating booking: {ex}");
                return (false, "Đã xảy ra lỗi: " + ex.Message, null);

            }
        }

          
        
        public async Task<(bool success, string? message)> UpdateAsync(string id, UpdateAppointDto updated)
        {
            var booking = await _repository.GetByIdAsync(id);
            if (booking == null) return (false, "Không tìm thấy lịch hẹn.");

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

                // Invoices & InvoiceDetails
                var invoices = await _context.Invoices
                    .Where(i => i.BookingId == id)
                    .ToListAsync();

                var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();
                var invoiceDetails = await _context.InvoiceDetails
                    .Where(d => invoiceIds.Contains(d.InvoiceId))
                    .ToListAsync();
                if (invoiceDetails.Any())
                    _context.InvoiceDetails.RemoveRange(invoiceDetails);
                if (invoices.Any())
                    _context.Invoices.RemoveRange(invoices);

                // Kits
                var kits = await _context.Kits.Where(f => f.BookingId == id).ToListAsync();
                if (kits.Any())
                    _context.Kits.RemoveRange(kits);

                // TestResults
                var testResults = await _context.TestResults.Where(t => t.BookingId == id).ToListAsync();
                if (testResults.Any())
                    _context.TestResults.RemoveRange(testResults);
                var relatives = await _context.Relatives.Where(t => t.BookingId == id).ToListAsync();
                if (relatives.Any())
                    _context.Relatives.RemoveRange(relatives);

                // Cuối cùng: Booking
                _context.Bookings.Remove(booking);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Xóa lịch hẹn và tất cả dữ liệu liên quan thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi khi xóa lịch hẹn: {ex.Message}");
            }
        }
        


        public Task<List<DateTime>> GetAvailableSchedulesAsync()
            => _repository.GetAvailableSchedulesAsync();
    }
}