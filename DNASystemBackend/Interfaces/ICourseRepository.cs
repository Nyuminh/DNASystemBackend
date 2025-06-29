using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(string courseId);
        Task<IEnumerable<Course>> GetAllAsync();
        Task<IEnumerable<Course>> GetByManagerIdAsync(string managerId);
        Task<bool> TitleExistsAsync(string title);
        Task AddAsync(Course course);
        Task UpdateAsync(string id,Course course);
        Task DeleteAsync(string courseId);
        Task SaveAsync();
    }
}