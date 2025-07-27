using System.Collections.Generic;
using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<(bool success, string? message)> CreateCourseAsync([FromForm] CreateCourseDto course)
        {
            
            var newCourse = new Course
            {
                ManagerId = course.ManagerId,
                Description = course.Description,
                Title = course.Title,
                Date = course.Date,
            };
            if(string.IsNullOrEmpty(newCourse.CourseId))
            {
                newCourse.CourseId = await GenerateUniqueCourseIdAsync();
            }
            if (string.IsNullOrEmpty(newCourse.ManagerId))
            {
                return (false, "ManagerId không được để trống.");
            }
            if (course.picture != null && course.picture.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", course.picture.FileName);
                using (var stream = System.IO.File.Create(path))
                {
                    await course.picture.CopyToAsync(stream);
                }
                newCourse.Image = "/images/" + course.picture.FileName; // Assuming you want to store the filename in the database
            }


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

        public async Task<(bool success, string? message)> UpdateCourseAsync( string courseId, [FromForm] UpdateCourseDto updateCourseDto)
        {
            var course = await _repository.GetByIdAsync(courseId);
            if (course == null)
                return (false, "Không tìm thấy khóa học.");

            course.Description = updateCourseDto.Description;
            course.Title = updateCourseDto.Title;
            course.Date = updateCourseDto.Date;
            
            if (updateCourseDto.picture != null && updateCourseDto.picture.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", updateCourseDto.picture.FileName);
                using (var stream = System.IO.File.Create(path))
                {
                    await updateCourseDto.picture.CopyToAsync(stream);
                }
                course.Image = "/images/" + updateCourseDto.picture.FileName; // Assuming you want to store the filename in the database
            }
            try
            {
                await _repository.UpdateAsync(courseId, course);
                await _repository.SaveAsync();
                return (true, "Cập nhật khóa học thành công.");
            }
            catch (Exception ex)
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

        public Task<IEnumerable<Course>> GetAllCoursesAsync()
            => _repository.GetAllAsync();


        public Task<Course?> GetCourseByIdAsync(string courseId)
        {
            return _repository.GetByIdAsync(courseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByManagerIdAsync(string managerId)
        {
            return await _repository.GetByManagerIdAsync(managerId);
        }


        private async Task<string> GenerateUniqueCourseIdAsync()
        {
            var existingIds = await _context.Courses
                .Select(c => c.CourseId)
                .Where(id => id.StartsWith("C") && id.Length == 4)
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