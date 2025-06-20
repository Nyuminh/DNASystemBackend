using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _service;

        public FeedbacksController(IFeedbackService service)
        {
            _service = service;
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

        [HttpPost]
        public async Task<ActionResult<Feedback>> CreateFeedback([FromBody] Feedback feedback)
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
