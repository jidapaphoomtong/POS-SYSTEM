using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace backend.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly FirestoreDB _firestoreDb;

        public NotificationService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        
        public async Task NotifyLowStock(string branchId, string productId, int newStock)
        {
            if (string.IsNullOrEmpty(branchId) || string.IsNullOrEmpty(productId))
            {
                Console.WriteLine("Branch ID or Product ID is null/empty.");
                return; // หลีกเลี่ยงการทำงานต่อ
            }

            var message = $"Stock for Product ID {productId} is low.";
            var notification = new Notification
            {
                Message = message,
                Timestamp = DateTime.UtcNow,
                BranchId = branchId,
                ProductId = productId
            };

            Console.WriteLine(JsonConvert.SerializeObject(notification));

            // บันทึกการแจ้งเตือนใน Firestore
            await _firestoreDb
                .Collection(FirestoreCollections.Branches)
                .Document(branchId)
                .Collection(FirestoreCollections.Notifications)
                .AddAsync(notification);
            
            Console.WriteLine($"Notification added: {message}"); // Log สำหรับตรวจสอบ
        }

        public async Task<List<Notification>> GetNotificationsAsync(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
            {
                Console.WriteLine("Branch ID is null or empty.");
                return new List<Notification>();
            }

            var notifications = new List<Notification>();
            Console.WriteLine($"Fetching notifications for branch ID: {branchId}");

            var query = _firestoreDb
                .Collection(FirestoreCollections.Branches)
                .Document(branchId)
                .Collection(FirestoreCollections.Notifications);

            var querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Count == 0)
            {
                Console.WriteLine($"No notifications found for branch ID: {branchId}");
                return notifications;
            }

            foreach (var document in querySnapshot.Documents)
            {
                var notification = document.ConvertTo<Notification>();
                // Console.WriteLine($"Fetched notification: {JsonConvert.SerializeObject(notification)}");

                if (notification != null)
                {
                    notifications.Add(notification);
                }
                else
                {
                    Console.WriteLine($"Notification from document {document.Id} is null.");
                }
            }

            return notifications;
        }

        public async Task<IActionResult> MarkAsRead(string branchId, string notificationId)
        {
            try
            {
                var notificationRef = _firestoreDb
                .Collection(FirestoreCollections.Branches)
                .Document(branchId)
                .Collection(FirestoreCollections.Notifications)
                .Document(notificationId);
                    
                await notificationRef.SetAsync(new { IsRead = true }, SetOptions.MergeAll);
                return new OkObjectResult(new { Success = true, Message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { Message = ex.Message }) { StatusCode = 500 };
            }
        }

        public async Task<bool> DeleteAllNotificationsAsync(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
            {
                return false; // ต้องแน่ใจว่ามี Branch ID
            }

            try
            {
                var notificationsCollection = _firestoreDb
                .Collection(FirestoreCollections.Branches)
                .Document(branchId)
                .Collection(FirestoreCollections.Notifications);

                var querySnapshot = await notificationsCollection.GetSnapshotAsync();
                var deleteTasks = querySnapshot.Documents.Select(document => document.Reference.DeleteAsync());

                await Task.WhenAll(deleteTasks); // รอจนกว่าการลบทั้งหมดเสร็จสิ้น

                return true; // การลบทั้งหมดสำเร็จ
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all notifications: {ex.Message}");
                return false; // การลบล้มเหลว
            }
        }

        // private readonly List<Notification> _temporaryNotifications = new List<Notification>();

        // public Task NotifyLowStock(string branchId, string productId)
        // {
        //     var message = $"Stock for Product ID {productId} is low.";
        //     var notification = new Notification
        //     {
        //         Message = message,
        //         Timestamp = DateTime.UtcNow,
        //         BranchId = branchId,
        //         ProductId = productId
        //     };
            
        //     // บันทึกการแจ้งเตือนชั่วคราว
        //     _temporaryNotifications.Add(notification);
        //     return Task.CompletedTask; // เนื่องจากเราไม่ต้องการรอคอยผลลัพธ์
        // }

        // public Task<List<Notification>> GetNotificationsAsync(string branchId)
        // {
        //     // คืนค่าการแจ้งเตือนที่เกี่ยวข้องกับ branchId
        //     var notifications = _temporaryNotifications
        //         .Where(n => n.BranchId == branchId)
        //         .ToList();
        //     return Task.FromResult(notifications); // คืนค่าการแจ้งเตือน
        // }
    }
}