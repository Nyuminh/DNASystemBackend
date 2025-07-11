using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Repositories
{
    public class RelativeRepository : IRelativeRepository
    {
        private readonly DnasystemContext _context;

        public RelativeRepository(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Relative>> GetAllAsync()
        {
            return await _context.Relatives.ToListAsync();
        }

        public async Task<Relative?> GetByIdAsync(string id)
        {
            return await _context.Relatives.FindAsync(id);
        }
        public async Task<Relative?> GetByUserIdAsync(string userId)
        {
            return await _context.Relatives
                .Where(r => r.UserId == userId)
                .FirstOrDefaultAsync();
        }
        public async Task<Relative?> GetByBookingIdAsync(string bookingId)
        {
            return await _context.Relatives
                .Where(r => r.BookingId == bookingId)
                .FirstOrDefaultAsync();
        }
        public async Task<Relative> CreateAsync(Relative relative)
        {
            _context.Relatives.Add(relative);
            await _context.SaveChangesAsync();
            return relative;
        }

        public async Task<bool> UpdateAsync(Relative relative)
        {
            _context.Relatives.Update(relative);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.Relatives.FindAsync(id);
            if (entity == null) return false;

            _context.Relatives.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
