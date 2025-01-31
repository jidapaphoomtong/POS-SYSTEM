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

        public async Task<string> GetNextId(string sequenceName)
        {
            try
            {
                var sequenceDoc = _firestoreDb.Collection(FirestoreCollections.Config).Document(sequenceName);
                var snapshot = await sequenceDoc.GetSnapshotAsync();

                int counter = 1;

                if (snapshot.Exists && snapshot.TryGetValue<int>("counter", out var currentCounter))
                {
                    counter = currentCounter;
                }

                // Increment ลำดับ
                await sequenceDoc.SetAsync(new { counter = counter + 1 });

                // คืนค่า ID ในรูปแบบ "001", "002", "003"
                return counter.ToString("D3");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate ID for {sequenceName}: {ex.Message}");
            }
        }
        
        public async Task NotifyLowStock(string branchId, string productId, int newStock)
        {
            if (string.IsNullOrEmpty(branchId) || string.IsNullOrEmpty(productId))
            {
                Console.WriteLine("Branch ID or Product ID is null/empty.");
                return; 
            }

            var message = $"Stock for Product ID {productId} is low.";
            
            // สร้าง ID ใหม่
            var notificationId = await GetNextId($"notification-sequence-{branchId}");

            var notification = new Notification
            {
                Id = notificationId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                BranchId = branchId,
                ProductId = productId,
                IsRead = false // กำหนดค่าตั้งต้นเป็น false
            };

            Console.WriteLine(JsonConvert.SerializeObject(notification));

            await _firestoreDb
                .Collection(FirestoreCollections.Branches)
                .Document(branchId)
                .Collection(FirestoreCollections.Notifications)
                .Document(notificationId) // ใช้ ID ที่สร้างมา
                .SetAsync(notification); // บันทึกการแจ้งเตือน

            Console.WriteLine($"Notification added: {message}");
        }

        public async Task NotifyOutOfStock(string branchId, string productId)
        {
            if (string.IsNullOrEmpty(branchId) || string.IsNullOrEmpty(productId))
            {
                Console.WriteLine("Branch ID or Product ID is null/empty.");
                return;
            }

            var message = $"Product ID {productId} is out of stock.";
            var notification = new Notification
            {
                Id = await GetNextId($"notification-sequence-{branchId}"),
                Message = message,
                Timestamp = DateTime.UtcNow,
                BranchId = branchId,
                ProductId = productId,
                IsRead = false
            };

            Console.WriteLine(JsonConvert.SerializeObject(notification));

            // บันทึกการแจ้งเตือนใน Firestore
            await _firestoreDb
                .Collection(FirestoreCollections.Branches)
                .Document(branchId)
                .Collection(FirestoreCollections.Notifications)
                .Document(notification.Id)
                .SetAsync(notification);

            Console.WriteLine($"Notification added: {message}");
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

                // อัปเดต IsRead เป็น true
                await notificationRef.SetAsync(new { IsRead = true }, SetOptions.MergeAll);
                return new OkObjectResult(new { Success = true, Message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { Message = ex.Message }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> MarkAllAsRead(string branchId)
        {
            try
            {
                // ดึงการแจ้งเตือนทั้งหมด
                var notificationsCollection = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Notifications);

                var querySnapshot = await notificationsCollection.GetSnapshotAsync();

                // อัปเดตเป็นอ่านสำหรับการแจ้งเตือนทั้งหมด
                foreach (var document in querySnapshot.Documents)
                {
                    await document.Reference.SetAsync(new { IsRead = true }, SetOptions.MergeAll);
                }

                return new OkObjectResult(new { Success = true, Message = "All notifications marked as read." });
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