using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultsController : ControllerBase
    {
        private readonly ITestResultService _service;

        public TestResultsController(ITestResultService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetResults()
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestResult>> GetResult(string id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TestResult>> CreateResult([FromBody] TestResult result)
        {
            var created = await _service.CreateAsync(result);
            return CreatedAtAction(nameof(GetResult), new { id = created.ResultId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(string id, [FromBody] TestResult updated)
        {
            if (id != updated.ResultId)
                return BadRequest("ID không khớp.");

            var success = await _service.UpdateAsync(id, updated);
            return success ? Ok(new { message = "Cập nhật thành công." }) : NotFound();
        }
    }
}
