using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    public class UpdateModel
        {
            public string Name { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; } // ใช้ Nullable Time หากไม่ต้องการบังคับ
            public string Description { get; set; } = string.Empty;
        }

    [FirestoreData]
    public class UpdateUser
    {
        [FirestoreProperty]
        public string firstName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string lastName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string email { get; set; } = string.Empty;
        [FirestoreProperty]
        public string password { get; set; } = string.Empty;
        // public IList<Role> role { get; set; } = new List<Role>();
    }
}