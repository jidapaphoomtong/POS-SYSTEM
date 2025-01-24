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
        public string Id { get; set; } = string.Empty; // ID ของการแจ้งเตือน
        [FirestoreProperty]
        public string Message { get; set; } = string.Empty;
        [FirestoreProperty]
        public bool Read { get; set; }
        [FirestoreProperty]
        public DateTime Timestamp { get; set; } = DateTime.Now; // Timestamp อัตโนมัติ
        [FirestoreProperty]
        public string BranchId { get; set; } = string.Empty; // ID ของสาขาที่เกี่ยวข้อง
        [FirestoreProperty]
        public string Role { get; set; } = string.Empty; // บทบาทของผู้ใช้ที่เกี่ยวข้อง
        [FirestoreProperty]
        public bool IsVisible { get; set; } = true; // เพื่อกำหนดการเข้าถึง
    }
}