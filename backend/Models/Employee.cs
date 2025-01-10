using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Employee
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty; // ไอดีพนักงาน
        [FirestoreProperty]
        public string firstName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string lastName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string email { get; set; } = string.Empty;
        [FirestoreProperty]
        public string password { get; set; } = string.Empty;
        [FirestoreProperty]
        public IList<Role> role { get; set; } = new List<Role>(); // กำหนดค่า Role นับตั้งแต่เริ่ม
        public string branchId { get; set; } // เพิ่มฟิลด์สำหรับ branchId
        // public string Salt { get; set; } // เพิ่ม property นี้
        // [FirestoreProperty]
        // public Timestamp CreatedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
        // [FirestoreProperty]
        // public Timestamp UpdatedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
    }
}