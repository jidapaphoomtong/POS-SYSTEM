using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.NotificationService
{
    public interface INotificationService
    {
        Task NotifyLowStock(string branchId, string productId, int newStock);
        Task<List<Notification>> GetNotificationsAsync(string branchId);
        Task<bool> DeleteAllNotificationsAsync(string branchId);
        Task<IActionResult> MarkAsRead(string branchId, string notificationId);
        Task<IActionResult> MarkAllAsRead(string branchId);
        Task NotifyOutOfStock(string branchId, string productId);
    }
}