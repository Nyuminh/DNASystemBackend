using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
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

        [HttpPost]
        public async Task<ActionResult<Kit>> CreateKit([FromBody] Kit kit)
        {
            var created = await _service.CreateAsync(kit);
            return CreatedAtAction(nameof(GetKit), new { id = created.KitId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
        {
            var success = await _service.UpdateStatusAsync(id, status);
            return success ? Ok(new { message = "Cập nhật trạng thái thành công." }) : NotFound();
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
