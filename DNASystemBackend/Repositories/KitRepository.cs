using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Repositories
{
    public class KitRepository : IKitRepository
    {
        private readonly DnasystemContext _context;

        public KitRepository(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Kit>> GetAllAsync()
        {
            return await _context.Kits
                .Include(k => k.Customer)
                .Include(k => k.Staff)
                .ToListAsync();
        }

        public async Task<Kit?> GetByIdAsync(string id)
        {
            return await _context.Kits
                .Include(k => k.Customer)
                .Include(k => k.Staff)
                .FirstOrDefaultAsync(k => k.KitId == id);
        }

        public async Task<Kit> CreateAsync(Kit kit)
        {
            kit.KitId = await GenerateKitIdAsync();
            _context.Kits.Add(kit);
            await _context.SaveChangesAsync();
            return kit;
        }

        public async Task<bool> UpdateStatusAsync(string id, Kit status)
        {
            var kit = await _context.Kits.FindAsync(id);
            if (kit == null) return false;

            kit.Status = status.Status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Kit>> GetTrackingSamplesAsync()
        {
            return await _context.Kits
                .Where(k => k.Status != null && k.Status != "Collected")
                .ToListAsync();
        }

        public async Task<IEnumerable<Kit>> GetCollectionSamplesAsync()
        {
            return await _context.Kits
                .Where(k => k.Status == null || k.Status == "Pending")
                .ToListAsync();
        }

        public async Task<string> GenerateKitIdAsync()
        {
            var existingIds = await _context.Kits.Select(k => k.KitId).ToListAsync();
            int counter = 1;
            string newId;

            do
            {
                newId = $"K{counter:D3}";
                counter++;
            } while (existingIds.Contains(newId));

            return newId;
        }
    }
}
