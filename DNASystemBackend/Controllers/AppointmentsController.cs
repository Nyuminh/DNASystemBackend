using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAppointments()
        {
            var bookings = await _service.GetAllAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetAppointment(string id)
        {
            var booking = await _service.GetByIdAsync(id);
            return booking == null ? NotFound() : Ok(booking);
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateAppointment(Booking booking)
        {
            var created = await _service.CreateAsync(booking);
            return CreatedAtAction(nameof(GetAppointment), new { id = created.BookingId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(string id, Booking updated)
        {
            if (id != updated.BookingId)
                return BadRequest("ID không khớp.");

            var result = await _service.UpdateAsync(id, updated);
            return result ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(string id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok(new { message = "Hủy lịch hẹn thành công." }) : NotFound();
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableSchedules()
        {
            var dates = await _service.GetAvailableSchedulesAsync();
            return Ok(dates);
        }
    }
}
