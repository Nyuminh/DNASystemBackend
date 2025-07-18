using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route ("api/[controller]")]
    [ApiController]
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
            try
            {
                var courses = await _courseRepository.GetAllAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("manager/{managerId}")]
        public async Task<IActionResult> GetCoursesByManagerId(string managerId)
        {
            var courses = await _courseRepository.GetByManagerIdAsync(managerId);
            return Ok(courses);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCourse([FromForm] CreateCourseDto course)
        {
            if (await _courseRepository.TitleExistsAsync(course.Title))
            {
                return BadRequest("Tựa đề khóa học đã tồn tại.");
            }
            var (success, message) = await _service.CreateCourseAsync(course);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Tạo khóa học thành công." });
        }

        [HttpPut("{courseId}")]
        [Authorize(Roles = "Manager")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCourse(string courseId, [FromForm] UpdateCourseDto course)
        {
            var (success, message) = await _service.UpdateCourseAsync(courseId, course);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Cập nhật khóa học thành công." });
        }

        [HttpDelete("{courseId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteCourse(string courseId)
        {
            var (success, message) = await _service.DeleteCourseAsync(courseId);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Xóa khóa học thành công." });
        }
    }
}