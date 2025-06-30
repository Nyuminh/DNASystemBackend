using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface ITestResultRepository
    {
        Task<IEnumerable<TestResult>> GetAllAsync();
        Task<TestResult?> GetByIdAsync(string id);
        Task<TestResult> CreateAsync(TestResult result);
        Task<bool> UpdateAsync(string id, TestResult updated);
        Task<bool> DeleteAsync(string id);
        Task<string> GenerateIdAsync();
       
    }
}
