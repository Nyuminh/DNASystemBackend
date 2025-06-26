using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ITestResultService _service;

        public ResultsController(ITestResultService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetAll()
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestResult>> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<TestResult>> Create([FromBody] TestResultCreateDto dto)
        {
            var id = await _service.GenerateIdAsync();
            var result = new TestResult
            {
                ResultId = id,
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                ServiceId = dto.ServiceId,
                BookingId = dto.BookingId,
                Date = dto.Date,
                Description = dto.Description,
                Status = dto.Status
            };

            var created = await _service.CreateAsync(result);
            return CreatedAtAction(nameof(GetById), new { id = created.ResultId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Update(string id, [FromBody] TestResult updatedResult)
        {
            var success = await _service.UpdateAsync(id, updatedResult);
            return success ? Ok(new { message = "Cập nhật kết quả thành công." }) : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? Ok(new { message = "Xóa kết quả thành công." }) : NotFound();
        }
    }
}
