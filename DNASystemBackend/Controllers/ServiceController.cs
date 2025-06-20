
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
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

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _serviceService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _serviceService.GetByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Service model)
        {
            var result = await _serviceService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.ServiceId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, Service model)
        {
            var success = await _serviceService.UpdateAsync(id, model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _serviceService.DeleteAsync(id);
            return success ? Ok(new { message = "Xóa dịch vụ thành công." }) : NotFound();
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories() => Ok(await _serviceService.GetCategoriesAsync());
    }
}
