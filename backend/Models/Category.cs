using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Category
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;
        // [FirestoreProperty]
        // public string IconUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public string branchId { get; set; } // เพิ่มฟิลด์สำหรับ branchId
    }

}