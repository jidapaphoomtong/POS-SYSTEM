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
            _notificationService = notificationService; // เก็บ instance ของ INotificationService
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNotification([FromBody] Notification notification)
        {
            await _notificationService.CreateNotification(notification);
            return Ok(new { Success = true, Message = "Notification created successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(string branchId, string role)
        {
            var notifications = await _notificationService.GetNotifications(branchId, role);
            return Ok(notifications);
        }
        
        [HttpPut("mark-all-as-read/{branchId}")]
        public async Task<IActionResult> MarkAllAsRead(string branchId)
        {
            await _notificationService.MarkAllAsRead(branchId);
            return Ok("All notifications marked as read.");
        }
    }
}