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
        public async Task<IActionResult> NotifyLowStock(string branchId, string productId)
        {
            // ตรวจสอบให้แน่ใจว่า branchId และ productId ไม่ว่างเปล่า
            if (string.IsNullOrEmpty(branchId) || string.IsNullOrEmpty(productId))
            {
                return BadRequest(new { success = false, message = "Branch ID and Product ID are required." });
            }

            try
            {
                // เรียกใช้บริการเพื่อส่งการแจ้งเตือน
                await _notificationService.NotifyLowStock(branchId, productId);
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
                if (string.IsNullOrEmpty(branchId))
                {
                    return BadRequest(new { success = false, message = "Branch ID is required." });
                }

                var notifications = await _notificationService.GetNotificationsAsync(branchId);
                return Ok(new { success = true, data = notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error fetching notifications: {ex.Message}" });
            }
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