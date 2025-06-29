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

        public async Task<IEnumerable<TestResult>> GetAllAsync() => await _context.TestResults.ToListAsync();

        public async Task<TestResult?> GetByIdAsync(string id) => await _context.TestResults.FindAsync(id);

        public async Task<TestResult> CreateAsync(TestResult result)
        {
            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<bool> UpdateAsync(string id, TestResult updated)
        {
            var existing = await _context.TestResults.FindAsync(id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(updated);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _context.TestResults.FindAsync(id);
            if (result == null) return false;

            _context.TestResults.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }

        private static readonly object _idLock = new object();

        public async Task<string> GenerateIdAsync()
        {
            lock (_idLock)
            {
                var lastId = _context.TestResults
                    .Where(r => r.ResultId.StartsWith("R") && r.ResultId.Length == 5)
                    .OrderByDescending(r => r.ResultId)
                    .Select(r => r.ResultId)
                    .FirstOrDefault();

                int nextId = 1;
                if (!string.IsNullOrEmpty(lastId) && int.TryParse(lastId.Substring(1), out int lastNum))
                {
                    nextId = lastNum + 1;
                }

                return $"R{nextId:D4}";
            }
        }
       

    }
}
