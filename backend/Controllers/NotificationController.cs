using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.NotificationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [LogAction]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("notify-low-stock")]
        public async Task<IActionResult> NotifyLowStock(string branchId, string productId, int newStock)
        {
            // ตรวจสอบให้แน่ใจว่า branchId และ productId ไม่ว่างเปล่า
            if (string.IsNullOrEmpty(branchId) || string.IsNullOrEmpty(productId))
            {
                return BadRequest(new { success = false, message = "Branch ID and Product ID are required." });
            }

            try
            {
                // เรียกใช้บริการเพื่อส่งการแจ้งเตือน
                await _notificationService.NotifyLowStock(branchId, productId, newStock);
                return Ok(new { success = true, message = "Notification sent successfully." });
            }
            catch (Exception ex)
            {
                // จัดการข้อผิดพลาดที่อาจเกิดขึ้น
                return StatusCode(500, new { success = false, message = $"Error sending notification: {ex.Message}" });
            }
        }

        [HttpGet("notification/{branchId}")]
        public async Task<IActionResult> GetNotifications(string branchId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsAsync(branchId);
                if (!notifications.Any())
                {
                    return NotFound(new { Message = $"No notifications found for branch ID: {branchId}" });
                }

                return Ok(new { Success = true, Data = notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPut("read-notification/{branchId}/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(string branchId, string notificationId)
        {
            var result = await _notificationService.MarkAsRead(branchId, notificationId);
            return result;
        }

        [HttpPut("read-all-notifications/{branchId}")]
        public async Task<IActionResult> MarkAllAsRead(string branchId)
        {
            return await _notificationService.MarkAllAsRead(branchId);
        }

        [HttpDelete("delete-all-notifications/{branchId}")]
        public async Task<IActionResult> DeleteAllNotifications(string branchId)
        {
            bool result = await _notificationService.DeleteAllNotificationsAsync(branchId);
            
            if (result)
            {
                return Ok(new { success = true, message = "All notifications deleted successfully." });
            }
            else
            {
                return NotFound(new { success = false, message = "No notifications found for this branch." });
            }
        }
    }
}