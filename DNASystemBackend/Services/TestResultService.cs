using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;

namespace DNASystemBackend.Services
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepository _repository;

        public TestResultService(ITestResultRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<TestResult>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<TestResult?> GetByIdAsync(string id)
            => _repository.GetByIdAsync(id);

        public Task<TestResult> CreateAsync(TestResult result)
            => _repository.CreateAsync(result);

        public Task<bool> UpdateAsync(string id, TestResult updated)
            => _repository.UpdateAsync(id, updated);
    }
}
