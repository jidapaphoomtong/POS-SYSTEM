using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.NotificationService
{
    public interface INotificationService
    {
        Task CreateNotification(Notification notification);
        Task<List<Notification>> GetNotifications(string branchId, string role);
        Task MarkAllAsRead(string branchId);
    }
}