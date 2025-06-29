using System.Collections.Generic;
using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(string courseId);
        Task<(bool success, string? message)> CreateCourseAsync(CreateCourseDto course);
        Task<(bool success, string? message)> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto);
        Task<(bool success, string? message)> DeleteCourseAsync(string courseId);
        Task<IEnumerable<Course>> GetCoursesByManagerIdAsync(string managerId);
    }
}