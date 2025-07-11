using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Services
{
    public class RelativeService : IRelativeService
    {
        private readonly IRelativeRepository _repository;

        public RelativeService(IRelativeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Relative>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Relative?> GetByIdAsync(string id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<Relative?> GetByUserIdAsync(string userId)
        {
            return await _repository.GetByUserIdAsync(userId);
            
        }
        public async Task<Relative?> GetByBookingIdAsync(string bookingId)
        {
            return await _repository.GetByBookingIdAsync(bookingId);
               
        }
        public async Task<Relative> CreateAsync(RelativeCreateDto dto)
        {
            var newRelative = new Relative
            {
                RelativeId = Guid.NewGuid().ToString("N")[..6].ToUpper(),
                UserId = dto.UserId,
                Fullname = dto.Fullname,
                Relationship = dto.Relationship,
                Gender = dto.Gender,
                Birthdate = dto.Birthdate,
                Phone = dto.Phone,
                Address = dto.Address,
                BookingId=dto.BookingId
                
            };

            return await _repository.CreateAsync(newRelative);
        }

        public async Task<bool> UpdateAsync(string id, Relative updated)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Fullname = updated.Fullname;
            existing.Relationship = updated.Relationship;
            existing.Gender = updated.Gender;
            existing.Birthdate = updated.Birthdate;
            existing.Phone = updated.Phone;
            existing.Address = updated.Address;

            return await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
