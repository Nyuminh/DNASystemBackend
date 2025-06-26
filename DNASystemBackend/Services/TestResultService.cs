namespace DNASystemBackend.Services
{
    using DNASystemBackend.Interfaces;
    using DNASystemBackend.Models;
    using DNASystemBackend.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepository _repo;
        private readonly DnasystemContext _context;

        public TestResultService(ITestResultRepository repo, DnasystemContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<IEnumerable<TestResult>> GetByBookingIdAsync(string bookingId)
        {
            return await _context.TestResults
                .Where(r => r.BookingId == bookingId)
                .ToListAsync();
        }





        public Task<IEnumerable<TestResult>> GetAllAsync() => _repo.GetAllAsync();
        public Task<TestResult?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
        public Task<TestResult> CreateAsync(TestResult result) => _repo.CreateAsync(result);
        public Task<bool> UpdateAsync(string id, TestResult updated) => _repo.UpdateAsync(id, updated);
        public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);
        public Task<string> GenerateIdAsync() => _repo.GenerateIdAsync();
    }
}