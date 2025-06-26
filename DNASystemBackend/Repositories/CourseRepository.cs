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
            return await _context.Courses.FindAsync(courseId);
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task AddAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
        }

        public async Task UpdateAsync(string id,Course course)
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

        public Task<List<Course>> GetByManagerIdAsync(string managerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TitleExistsAsync(string title)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync()
        {
            return _context.SaveChangesAsync();
        }

    }
}