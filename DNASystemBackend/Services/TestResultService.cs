namespace DNASystemBackend.Services
{
    using DNASystemBackend.Interfaces;
    using DNASystemBackend.Models;
    using DNASystemBackend.Repositories;

    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepository _repo;

        public TestResultService(ITestResultRepository repo)
        {
            _repo = repo;
        }



        public Task<IEnumerable<TestResult>> GetAllAsync() => _repo.GetAllAsync();
        public Task<TestResult?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
        public Task<TestResult> CreateAsync(TestResult result) => _repo.CreateAsync(result);
        public Task<bool> UpdateAsync(string id, TestResult updated) => _repo.UpdateAsync(id, updated);
        public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);
        public Task<string> GenerateIdAsync() => _repo.GenerateIdAsync();
    }
}