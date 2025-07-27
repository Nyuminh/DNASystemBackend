using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KitController : ControllerBase
    {
        private readonly IKitService _service;

        public KitController(IKitService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kit>>> GetKits()
        {
            var kits = await _service.GetAllAsync();
            return Ok(kits);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Kit>> GetKit(string id)
        {
            var kit = await _service.GetByIdAsync(id);
            return kit == null ? NotFound() : Ok(kit);
        }
        [HttpGet("by-booking/{bookingId}")]
        public async Task<ActionResult<Kit>> GetByBookingId(string bookingId)
        {
            var result = await _service.GetByBookingIdAsync(bookingId);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy Kit lịch hẹn {bookingId}" });

            return Ok(result);
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Kit>> CreateKit([FromBody] CreateKitDto kit)
        {
            var (success, message) = await _service.CreateAsync(kit);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Tạo kit thành công." });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateKitDto status)
        {
            var (success, message) = await _service.UpdateStatusAsync(id, status);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Cập nhật trạng thái kit thành công." });
        }

        [HttpGet("tracking")]
        public async Task<ActionResult<IEnumerable<Kit>>> TrackSamples()
        {
            var kits = await _service.GetTrackingSamplesAsync();
            return Ok(kits);
        }

        [HttpGet("collection")]
        public async Task<ActionResult<IEnumerable<Kit>>> GetSamplesForCollection()
        {
            var kits = await _service.GetCollectionSamplesAsync();
            return Ok(kits);
        }
    }
}
