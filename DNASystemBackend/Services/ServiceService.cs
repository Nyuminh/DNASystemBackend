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

        public async Task<(bool success, string? message)> DeleteAsync(string id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);
                if (!result) return (false, "Không tìm thấy dịch vụ.");
                return (true, "Xóa dịch vụ thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa dịch vụ: {ex.Message}");
            }
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