using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Notification
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string Message { get; set; }
        [FirestoreProperty]
        public DateTime Timestamp { get; set; }
        [FirestoreProperty]
        public string BranchId { get; set; }
        [FirestoreProperty]
        public string ProductId { get; set; }
        [FirestoreProperty]
        public bool IsRead { get; set; } // เพิ่มฟิลด์นี้
    }
}