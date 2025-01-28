using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.NotificationService
{
    public interface INotificationService
    {
        Task NotifyLowStock(string branchId, string productId);
        Task<List<Notification>> GetNotificationsAsync(string branchId);
        Task<bool> DeleteAllNotificationsAsync(string branchId);
    }
}