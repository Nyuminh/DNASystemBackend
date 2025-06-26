using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    public class CourseController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseService _service;

        public CourseController(ICourseRepository courseRepository, ICourseService service)
        {
            _courseRepository = courseRepository;
            _service = service;
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourseById(string courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _courseRepository.GetAllAsync();
            return Ok(courses);
        }

        [HttpGet("manager/{managerId}")]
        public async Task<IActionResult> GetCoursesByManagerId(string managerId)
        {
            var courses = await _courseRepository.GetByManagerIdAsync(managerId);
            return Ok(courses);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto course)
        {
            if (await _courseRepository.TitleExistsAsync(course.Title))
            {
                return BadRequest("Course title already exists.");
            }
            var (success, message) = await _service.CreateCourseAsync(course);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Tạo course thành công." });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateCourse(string id, [FromBody] UpdateCourseDto course)
        {
            var (success, message) = await _service.UpdateCourseAsync(id,course);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Tạo course thành công." });
        }

        [HttpDelete("{courseId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteCourse(string courseId)
        {
            var (success, message) = await _service.DeleteCourseAsync(courseId);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Xóa course thành công." });
        }
    }
}