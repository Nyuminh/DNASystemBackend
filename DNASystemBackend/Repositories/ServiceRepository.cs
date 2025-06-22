using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly DnasystemContext _context;

        public ServiceRepository(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
            => await _context.Services.ToListAsync();

        public async Task<Service?> GetByIdAsync(string id)
            => await _context.Services.FindAsync(id);

        public async Task<Service> CreateAsync(Service service)
        {
            service.ServiceId ??= await GenerateServiceIdAsync();
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<bool> UpdateAsync(string id, Service updated)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;

            service.Name = updated.Name;
            service.Type = updated.Type;
            service.Price = updated.Price;
            service.Description = updated.Description;
            service.Image = updated.Image;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Services
                .Where(s => s.Type != null)
                .Select(s => s.Type!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<string> GenerateServiceIdAsync()
        {
            var existingIds = await _context.Services.Select(s => s.ServiceId).ToListAsync();
            int counter = 1;
            string newId;
            do
            {
                newId = $"S{counter:D3}";
                counter++;
            } while (existingIds.Contains(newId));
            return newId;
        }
    }
}