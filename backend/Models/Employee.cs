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
        
        [FirestoreProperty(Name = "firstName")]
        public string firstName { get; set; } = string.Empty;

        [FirestoreProperty(Name = "lastName")]
        public string lastName { get; set; } = string.Empty;

        [FirestoreProperty(Name = "email")]
        public string email { get; set; } = string.Empty;

        [FirestoreProperty(Name = "role")]
        public IList<Role> role { get; set; } = new List<Role>(); // กำหนดค่า Role นับตั้งแต่เริ่ม

        [FirestoreProperty(Name = "password")]
        public string password { get; set; } = string.Empty; // ค่าที่เข้ารหัสแล้ว
        public string branchId { get; set; } // เพิ่มฟิลด์สำหรับ branchId

    }
}