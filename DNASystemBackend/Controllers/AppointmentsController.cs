using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet("by-service/{serviceID}")]
        public async Task<ActionResult<Booking>> GetByServiceId(string serviceID)
        {
            var result = await _service.GetByServiceIdAsync(serviceID);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy {serviceID}" });

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateAppointment([FromBody] AppointmentDto dto)
        {
            var (success, message, createdBooking) = await _service.CreateAsync(dto); 

            if (!success)
                return BadRequest(message);

            return Ok(new
            {
                message = "Tạo lịch hẹn thành công.",
                data = createdBooking
            });
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAppointment(string id, UpdateAppointDto updated)
        {
            var (success, message) = await _service.UpdateAsync(id, updated);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Cập nhật lịch hẹn thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(string id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            if (!success) return BadRequest(message);
            return Ok(new { message });
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableSchedules()
        {
            var dates = await _service.GetAvailableSchedulesAsync();
            return Ok(dates);
        }
    }
}