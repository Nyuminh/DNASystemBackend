
using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        // GET: /api/services
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceService.GetAllAsync();
            return Ok(services);
        }

        // GET: /api/services/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(string id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            return service != null ? Ok(service) : NotFound("Không tìm thấy dịch vụ.");
        }

        // POST: /api/services
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateService([FromForm] ServiceDto model)
        {
            var (success, message) = await _serviceService.CreateAsync(model);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Tạo dịch vụ thành công." });
        }

        // PUT: /api/services/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateService(string id, [FromForm] UpdateServiceDto model)
        {
            var (success, message) = await _serviceService.UpdateAsync(id, model);
            if (!success) return BadRequest(message);
            return Ok(new { message });
        }

        // DELETE: /api/services/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteService(string id)
        {
            var (success, message) = await _serviceService.DeleteAsync(id);
            if (!success) return BadRequest(message);
            return Ok(new { message });
        }
        [HttpDelete("{id}/cascade")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteWithCascade(string id)
        {
            var result = await _serviceService.DeleteWithCascadeAsync(id);
            if (result.success)
                return Ok(new { message = result.message });

            return BadRequest(new { message = result.message });
        }

        // GET: /api/services/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _serviceService.GetCategoriesAsync();
            return Ok(categories);
        }
    }
}