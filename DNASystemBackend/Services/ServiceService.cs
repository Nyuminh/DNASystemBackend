using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
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

        public async Task<(bool success, string? message)> CreateAsync(ServiceDto model)
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
                    Image = model.Image
                };

                await _repository.CreateAsync(service);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo dịch vụ: {ex.Message}");
            }
        }

        public async Task<(bool success, string? message)> UpdateAsync(string id, UpdateServiceDto model)
        {
            var service = await _repository.GetByIdAsync(id);
            if (service == null) return (false, "Không tìm thấy dịch vụ.");

            // Update properties from UpdateServiceDto
            if (!string.IsNullOrEmpty(model.Type)) service.Type = model.Type;
            if (!string.IsNullOrEmpty(model.Name)) service.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Description)) service.Description = model.Description;
            if (model.Price.HasValue) service.Price = model.Price;
            if (!string.IsNullOrEmpty(model.Image)) service.Image = model.Image;

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

                // Delete related Bookings
                var bookings = await _context.Bookings.Where(b => b.ServiceId == id).ToListAsync();
                if (bookings.Any())
                    _context.Bookings.RemoveRange(bookings);

                // Delete related Feedbacks
                var feedbacks = await _context.Feedbacks.Where(f => f.ServiceId == id).ToListAsync();
                if (feedbacks.Any())
                    _context.Feedbacks.RemoveRange(feedbacks);

                // Delete related InvoiceDetails
                var invoiceDetails = await _context.InvoiceDetails.Where(i => i.ServiceId == id).ToListAsync();
                if (invoiceDetails.Any())
                    _context.InvoiceDetails.RemoveRange(invoiceDetails);

                // Delete related TestResults
                var testResults = await _context.TestResults.Where(t => t.ServiceId == id).ToListAsync();
                if (testResults.Any())
                    _context.TestResults.RemoveRange(testResults);

                // Now delete the service
                await _repository.DeleteAsync(id);

                await transaction.CommitAsync();
                return (true, "Xóa dịch vụ và dữ liệu liên quan thành công.");
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