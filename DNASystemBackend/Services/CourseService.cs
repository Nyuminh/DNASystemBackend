using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Services
{
    public class CourseService : ICourseService
    {
        private readonly DnasystemContext _context;
        private readonly ICourseRepository _repository;
        public CourseService(DnasystemContext context, ICourseRepository repository)
        {
            _context = context;
            _repository = repository;
        }
        public async Task<(bool success, string? message)> CreateCourseAsync(CreateCourseDto course)
        {
            string newUserId = await GenerateUniqueUserIdAsync();
            var newCourse = new Course
            {
                ManagerId = course.ManagerId,
                Description = course.Description,
                Title = course.Title,
                Date = course.Date,
                Image = course.Image,
            };

            try
            {
                await _repository.AddAsync(newCourse);
                await _repository.SaveAsync();
                return (true, "Tạo khóa học thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo khóa học: {ex.Message}");
            }
        }

        public async Task<(bool success, string? message)> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto)
        {
            var course = await _repository.GetByIdAsync(courseId);
            if (course == null)
                return (false, "Không tìm thấy khóa học.");
            var newCourse = new Course
            {
                ManagerId = updateCourseDto.ManagerId,
                Description = updateCourseDto.Description,
                Title = updateCourseDto.Title,
                Date = updateCourseDto.Date,
                Image = updateCourseDto.Image
            };
            try
            {
                await _repository.UpdateAsync(courseId, newCourse);
                await _repository.SaveAsync();
                return (true, "Cập nhật khóa học thành công.");
            }catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật khóa học: {ex.Message}");
            }
        }


        public async Task<(bool success, string? message)> DeleteCourseAsync(string courseId)
        {
            var course = await _repository.GetByIdAsync(courseId);
            if (course == null)
                return (false, "Không tìm thấy khóa học.");
            try
            { 
                await _repository.DeleteAsync(courseId);
                await _repository.SaveAsync();
                return (true, "Xóa khóa học thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa khóa học: {ex.Message}");
            }
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public Task<Course?> GetCourseByIdAsync(string courseId)
        {
            return _repository.GetByIdAsync(courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByManagerIdAsync(string managerId)
        {
            return await _repository.GetByManagerIdAsync(managerId);
        }


        private async Task<string> GenerateUniqueUserIdAsync()
        {
            var existingIds = await _context.Users
            .Select(u => u.UserId)
            .Where(id => id.StartsWith("C") && id.Length == 3)
            .ToListAsync();

        int counter = 1;
        string newId;
        do
        {
            newId = $"C{counter:D03}";
            counter++;
        } while (existingIds.Contains(newId) && counter < 1000);

        if (counter >= 1000)
        {
            newId = $"C{DateTime.Now.Ticks % 1000000:D06}";
        }

        return newId;
        }
    }
}