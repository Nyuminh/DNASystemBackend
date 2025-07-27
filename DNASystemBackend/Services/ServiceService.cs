using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repository;
        private readonly DnasystemContext _context;

        public ServiceService(IServiceRepository repository, DnasystemContext context)
        {
            _repository = repository;
            _context = context;
        }

        public Task<IEnumerable<Service>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Service?> GetByIdAsync(string id) => _repository.GetByIdAsync(id);

        public async Task<(bool success, string? message)> CreateAsync([FromForm] ServiceDto model)
        {
            try
            {
                var service = new Service
                {
                    ServiceId = await GenerateUniqueServiceIdAsync(),
                    Type = model.Type,
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                };
                if (model.picture != null && model.picture.Length > 0)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", model.picture.FileName);
                    using (var stream = System.IO.File.Create(path))
                    {
                        await model.picture.CopyToAsync(stream);
                    }
                    service.Image = "/images/" + model.picture.FileName; // Assuming you want to store the filename in the database
                }

                await _repository.CreateAsync(service);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo dịch vụ: {ex.Message}");
            }
        }

        public async Task<(bool success, string? message)> UpdateAsync(string id, [FromForm] UpdateServiceDto model)
        {
            var service = await _repository.GetByIdAsync(id);
            if (service == null) return (false, "Không tìm thấy dịch vụ.");

            // Update properties from UpdateServiceDto
            if (!string.IsNullOrEmpty(model.Type)) service.Type = model.Type;
            if (!string.IsNullOrEmpty(model.Name)) service.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Description)) service.Description = model.Description;
            if (model.Price.HasValue) service.Price = model.Price;
            if (model.picture != null && model.picture.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", model.picture.FileName);
                using (var stream = System.IO.File.Create(path))
                {
                    await model.picture.CopyToAsync(stream);
                }
                service.Image = "/images/" + model.picture.FileName; // Assuming you want to store the filename in the database
            }
            try
            {
                await _repository.UpdateAsync(id, service);
                return (true, "Cập nhật dịch vụ thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật dịch vụ: {ex.Message}");
            }
        }

        public async Task<(bool success, string? message)> DeleteWithCascadeAsync(string id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var service = await _repository.GetByIdAsync(id);
                if (service == null)
                    return (false, "Không tìm thấy dịch vụ.");

                // Lấy danh sách các bookings liên quan đến service
                var bookings = await _context.Bookings.Where(b => b.ServiceId == id).ToListAsync();
                var bookingIds = bookings.Select(b => b.BookingId).ToList();

                // 1. Xóa TestResult liên quan đến các booking
                var testResultsByBooking = await _context.TestResults
                    .Where(tr => bookingIds.Contains(tr.BookingId)).ToListAsync();
                if (testResultsByBooking.Any()) _context.TestResults.RemoveRange(testResultsByBooking);

                // 2. Xóa TestResult liên quan trực tiếp đến Service
                var testResultsByService = await _context.TestResults
                    .Where(tr => tr.ServiceId == id && !bookingIds.Contains(tr.BookingId ?? "")).ToListAsync();
                if (testResultsByService.Any()) _context.TestResults.RemoveRange(testResultsByService);

                // 3. Xóa Kits liên quan đến Booking
                var kits = await _context.Kits
                    .Where(k => bookingIds.Contains(k.BookingId)).ToListAsync();
                if (kits.Any()) _context.Kits.RemoveRange(kits);

                // 4. Xóa Invoices liên quan đến Booking
                var invoices = await _context.Invoices
                    .Where(i => bookingIds.Contains(i.BookingId)).ToListAsync();
                var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();

                // 5. Xóa InvoiceDetails liên quan đến Service
                var invoiceDetails = await _context.InvoiceDetails
                    .Where(d => d.ServiceId == id || invoiceIds.Contains(d.InvoiceId)).ToListAsync();
                if (invoiceDetails.Any()) _context.InvoiceDetails.RemoveRange(invoiceDetails);

                if (invoices.Any()) _context.Invoices.RemoveRange(invoices);

                // 6. Xóa Bookings
                if (bookings.Any()) _context.Bookings.RemoveRange(bookings);

                // 7. Xóa Feedbacks
                var feedbacks = await _context.Feedbacks.Where(f => f.ServiceId == id).ToListAsync();
                if (feedbacks.Any()) _context.Feedbacks.RemoveRange(feedbacks);

                // 8. Xóa Service
                await _repository.DeleteAsync(id);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Xóa dịch vụ và toàn bộ dữ liệu liên quan thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi khi xóa dịch vụ: {ex.Message}");
            }
        }


        public async Task<(bool success, string? message)> DeleteAsync(string id)
        {
            try
            {
                // Check for related records before deletion
                var service = await _repository.GetByIdAsync(id);
                if (service == null)
                    return (false, "Không tìm thấy dịch vụ.");

                // Check for dependencies
                bool hasBookings = await _context.Bookings.AnyAsync(b => b.ServiceId == id);
                bool hasFeedbacks = await _context.Feedbacks.AnyAsync(f => f.ServiceId == id);
                bool hasInvoiceDetails = await _context.InvoiceDetails.AnyAsync(i => i.ServiceId == id);
                bool hasTestResults = await _context.TestResults.AnyAsync(t => t.ServiceId == id);

                if (hasBookings || hasFeedbacks || hasInvoiceDetails || hasTestResults)
                {
                    string dependencies = GetDependencyList(id, hasBookings, hasFeedbacks, hasInvoiceDetails, hasTestResults);
                    return (false, $"Không thể xóa dịch vụ vì có dữ liệu liên quan: {dependencies}");
                }

                // No related records found, proceed with deletion
                var result = await _repository.DeleteAsync(id);
                if (!result)
                    return (false, "Không tìm thấy dịch vụ.");

                return (true, "Xóa dịch vụ thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa dịch vụ: {ex.Message}");
            }
        }
        private string GetDependencyList(string serviceId, bool hasBookings, bool hasFeedbacks,
    bool hasInvoiceDetails, bool hasTestResults)
        {
            var dependencies = new List<string>();

            if (hasBookings)
                dependencies.Add($"Lịch hẹn ({_context.Bookings.Count(b => b.ServiceId == serviceId)} bản ghi)");

            if (hasFeedbacks)
                dependencies.Add($"Đánh giá ({_context.Feedbacks.Count(f => f.ServiceId == serviceId)} bản ghi)");

            if (hasInvoiceDetails)
                dependencies.Add($"Chi tiết hóa đơn ({_context.InvoiceDetails.Count(i => i.ServiceId == serviceId)} bản ghi)");

            if (hasTestResults)
                dependencies.Add($"Kết quả xét nghiệm ({_context.TestResults.Count(t => t.ServiceId == serviceId)} bản ghi)");

            return string.Join(", ", dependencies);
        }

        public Task<List<string>> GetCategoriesAsync() => _repository.GetCategoriesAsync();

        private async Task<string> GenerateUniqueServiceIdAsync()
        {
            var existingIds = await _context.Services
                .Select(s => s.ServiceId)
                .Where(id => id.StartsWith("S") && id.Length == 4)
                .ToListAsync();

            int counter = 1;
            string newId;
            do
            {
                newId = $"S{counter:D03}";
                counter++;
            } while (existingIds.Contains(newId) && counter < 1000);

            if (counter >= 1000)
            {
                newId = $"S{DateTime.Now.Ticks % 1000000:D06}";
            }

            return newId;
        }
    }
}