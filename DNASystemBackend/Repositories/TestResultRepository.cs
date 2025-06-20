using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Repositories
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly DnasystemContext _context;

        public TestResultRepository(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestResult>> GetAllAsync()
        {
            return await _context.TestResults
                .Include(r => r.Customer)
                .Include(r => r.Staff)
                .Include(r => r.Service)
                .ToListAsync();
        }

        public async Task<TestResult?> GetByIdAsync(string id)
        {
            return await _context.TestResults
                .Include(r => r.Customer)
                .Include(r => r.Staff)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.ResultId == id);
        }

        public async Task<TestResult> CreateAsync(TestResult result)
        {
            result.ResultId = await GenerateResultIdAsync();
            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<bool> UpdateAsync(string id, TestResult updated)
        {
            var result = await _context.TestResults.FindAsync(id);
            if (result == null) return false;

            result.Description = updated.Description;
            result.Status = updated.Status;
            result.Date = updated.Date;
            result.StaffId = updated.StaffId;
            result.ServiceId = updated.ServiceId;
            result.CustomerId = updated.CustomerId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateResultIdAsync()
        {
            var existingIds = await _context.TestResults.Select(r => r.ResultId).ToListAsync();
            int counter = 1;
            string newId;

            do
            {
                newId = $"R{counter:D3}";
                counter++;
            } while (existingIds.Contains(newId));

            return newId;
        }
    }
}
