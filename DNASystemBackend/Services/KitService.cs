using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Services
{
    public class KitService : IKitService
    {
        private readonly IKitRepository _repository;
        private readonly DnasystemContext _context;

        public KitService(IKitRepository repository, DnasystemContext context)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<Kit?> GetByBookingIdAsync(string bookingId)
        {
            return await _context.Kits
                .Where(r => r.BookingId == bookingId)
                .FirstOrDefaultAsync();
        }
        public Task<IEnumerable<Kit>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<Kit?> GetByIdAsync(string id)
            => _repository.GetByIdAsync(id);

        public Task<IEnumerable<Kit>> GetTrackingSamplesAsync()
            => _repository.GetTrackingSamplesAsync();

        public Task<IEnumerable<Kit>> GetCollectionSamplesAsync()
            => _repository.GetCollectionSamplesAsync();

        public async Task<(bool success, string? message)> CreateAsync(CreateKitDto kit)
        {
            try
            {
                var newKit = new Kit
                {
                    CustomerId = kit.CustomerId,
                    StaffId = kit.StaffId,
                    BookingId=kit.BookingId,
                    Description = kit.Description,
                    Status = kit.Status,
                    Receivedate = kit.Receivedate,
                };
                if (string.IsNullOrEmpty(newKit.KitId))
                {
                    newKit.KitId = await _repository.GenerateKitIdAsync();
                }
                else
                {
                    newKit.KitId = kit.KitId;
                }
                await _repository.CreateAsync(newKit);
                return (true, "Đã tạo kit thành công");
                
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo kit: {ex.Message}");
            }
        }

        public async Task<(bool success, string? message)> UpdateStatusAsync(string id, UpdateKitDto status)
        {
            var kit = await _repository.GetByIdAsync(id);
            if (kit == null) return (false, "Không tìm thấy kit.");

            kit.Status = status.Status;

            try
            {
                await _repository.UpdateStatusAsync(id, kit);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật trạng thái kit thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật trạng thái kit: {ex.Message}");
            }
        }

        public async Task<(bool success, string? message)> DeleteAsync(string id)
        {
            var kit = await _repository.GetByIdAsync(id);
            if (kit == null) return (false, "Không tìm thấy kit.");
            try
            {
                _context.Kits.Remove(kit);
                await _context.SaveChangesAsync();
                return (true, "Xóa kit thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa kit: {ex.Message}");
            }
        }

    }
}
