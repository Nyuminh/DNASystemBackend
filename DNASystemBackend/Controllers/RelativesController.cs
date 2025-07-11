using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelativesController : ControllerBase
    {
        private readonly IRelativeService _service;

        public RelativesController(IRelativeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Relative>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }
        [HttpGet("by-booking/{bookingId}")]
        public async Task<ActionResult<Relative>> GetByBookingId(string bookingId)
        {
            var result = await _service.GetByBookingIdAsync(bookingId);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy Relatives cho lịch hẹn {bookingId}" });

            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Relative>> GetById(string id)
        {
            var relative = await _service.GetByIdAsync(id);
            return relative == null ? NotFound() : Ok(relative);
        }
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<Relative>> GetByUserId(string userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy {userId}" });

            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<Relative>> Create([FromBody] RelativeCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.RelativeId }, created);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Relative updated)
        {
            var success = await _service.UpdateAsync(id, updated);
            return success ? Ok(new { message = "Cập nhật thành công." }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? Ok(new { message = "Xóa thành công." }) : NotFound();
        }
    }
}
