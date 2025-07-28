using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _service;
        private readonly IFeedbackRepository _repo;

        public FeedbacksController(IFeedbackService service , IFeedbackRepository repo)
        {
            _service = service;
            _repo= repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbacks()
        {
            var feedbacks = await _service.GetAllAsync();
            return Ok(feedbacks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Feedback>> GetFeedback(string id)
        {
            var feedback = await _service.GetByIdAsync(id);
            return feedback == null ? NotFound() : Ok(feedback);
        }
        [HttpGet("by-service/{serviceID}")]
        public async Task<ActionResult<Feedback>> GetByServiceId(string serviceID)
        {
            var result = await _repo.GetByServiceIdAsync(serviceID);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy {serviceID}" });

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Feedback>> CreateFeedback([FromBody] CreateFeedbackDto feedback)
        {
            var created = await _service.CreateAsync(feedback);
            return CreatedAtAction(nameof(GetFeedback), new { id = created.FeedbackId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(string id, [FromBody] Feedback updated)
        {
            if (id != updated.FeedbackId)
                return BadRequest("ID không khớp.");

            var success = await _service.UpdateAsync(id, updated);
            return success ? Ok(new { message = "Cập nhật thành công." }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? Ok(new { message = "Xóa thành công." }) : NotFound();
        }
    }
}
