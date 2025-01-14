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
        // [FirestoreProperty]
        // public string phone { get; set; } = string.Empty; // Phone
        // [FirestoreProperty]
        // public int age { get; set; } // Age
        [FirestoreProperty]
        public string passwordHash { get; set; } = string.Empty;
        [FirestoreProperty]
        public IList<Role> roles { get; set; } = new List<Role>(); // กำหนดค่า Role นับตั้งแต่เริ่ม
        [FirestoreProperty]
        public string branchId { get; set; } // เพิ่มฟิลด์สำหรับ branchId
        [FirestoreProperty]
        public string salt { get; set; } // เพิ่ม property นี้
    }
}