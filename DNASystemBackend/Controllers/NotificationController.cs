using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DnaTestingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest(new { message = "Notification ID không được để trống" });

                var notification = await _notificationService.GetNotificationAsync(id);
                return Ok(notification);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Lỗi xuất hiện: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "User ID không được để trống." });

                var notifications = await _notificationService.GetNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Lỗi xuất hiện: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationRequestDTO request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Message))
                    return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });

                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    NotificationType = request.NotificationType,
                };

                var createdNotification = await _notificationService.CreateNotificationAsync(notification);
                return CreatedAtAction(nameof(GetNotification), new { id = createdNotification.NotificationId }, createdNotification);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Lỗi xuất hiện: {ex.Message}" });
            }
        }

        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest(new { message = "Notification ID không được để trống." });

                var notification = await _notificationService.GetNotificationAsync(id);
                notification.IsRead = true;
                await _notificationService.UpdateNotificationAsync(notification);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Lỗi xuất hiện: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest(new { message = "Notification ID không được để trống." });

                await _notificationService.DeleteNotificationAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Lỗi xuất hiện: {ex.Message}" });
            }
        }
    }
}