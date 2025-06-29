using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly DnasystemContext _context;

        public CourseRepository(DnasystemContext context)
        {
            _context = context;
        }

        public async Task<Course?> GetByIdAsync(string courseId)
        {
            return await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetByManagerIdAsync(string managerId)
        {
            return await _context.Courses
                .Where(c => c.ManagerId == managerId)
                .ToListAsync();
        }

        public async Task<bool> TitleExistsAsync(string title)
        {
            return await _context.Courses.AnyAsync(c => c.Title == title);
        }

        public async Task AddAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
        }

        public async Task UpdateAsync(string courseId,Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string courseId)
        {
            var course = await GetByIdAsync(courseId);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public Task SaveAsync()
        {
            return _context.SaveChangesAsync();
        }

    }
}