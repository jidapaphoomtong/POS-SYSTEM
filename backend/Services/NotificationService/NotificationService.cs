using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;

namespace backend.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly FirestoreDb _firestoreDb;

        public NotificationService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        
        public async Task<string> GetNextId(string sequenceName)
        {
            try
            {
                var sequenceDoc = _firestoreDb.Collection("config").Document(sequenceName);
                var snapshot = await sequenceDoc.GetSnapshotAsync();

                int counter = 1; // Default counter value

                // If the sequence document exists, update the counter
                if (snapshot.Exists && snapshot.TryGetValue("counter", out int currentCounter))
                {
                    counter = currentCounter;
                }

                // Increment the sequence counter
                await sequenceDoc.SetAsync(new { counter = counter + 1 });

                // Return the formatted ID as "001", "002", "003", etc.
                return counter.ToString("D3");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate ID for {sequenceName}: {ex.Message}");
            }
        }

        // Method to create a notification
        public async Task CreateNotification(Notification notification)
        {
            string notiId = await GetNextId($"noti-sequence-{notification.BranchId}"); 
            notification.Id = notiId;

            DocumentReference docRef = _firestoreDb.Collection("notifications").Document(notification.Id);
            await docRef.SetAsync(notification);
        }

        public async Task<List<Notification>> GetNotifications(string branchId, string role)
        {
            Query query = _firestoreDb.Collection("notifications").WhereEqualTo("BranchId", branchId);

            if (role != "Admin")
            {
                query = query.WhereEqualTo("Role", role);
            }

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            List<Notification> notifications = new List<Notification>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Notification notification = document.ConvertTo<Notification>();
                    notifications.Add(notification);
                }
            }

            return notifications;
        }

        public async Task MarkAllAsRead(string branchId)
        {
            var query = _firestoreDb.Collection("notifications").WhereEqualTo("BranchId", branchId);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var notification = document.ConvertTo<Notification>();
                    notification.Read = true; 

                    DocumentReference docRef = _firestoreDb.Collection("notifications").Document(notification.Id);
                    await docRef.SetAsync(notification, SetOptions.MergeAll);
                }
            }
        }
    }
}