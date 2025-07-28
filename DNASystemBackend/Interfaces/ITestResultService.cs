namespace DNASystemBackend.Interfaces
{
    using DNASystemBackend.DTOs;
    using DNASystemBackend.Models;

    public interface ITestResultService
    {
        Task<IEnumerable<TestResult>> GetAllAsync();
        Task<TestResult?> GetByIdAsync(string id);
        Task<TestResult> CreateAsync(TestResult result);
        Task<bool> UpdateAsync(string id, UpdateTestResultDTO updated);
        Task<bool> DeleteAsync(string id);
        Task<string> GenerateIdAsync();
        Task<TestResult?> GetByBookingIdAsync(string bookingId);


    }
}