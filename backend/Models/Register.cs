using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Register
    {
        // public string userId { get; set; } = string.Empty;
        [FirestoreProperty]
        public string firstName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string lastName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string email { get; set; } = string.Empty;
        [FirestoreProperty]
        public string password { get; set; } = string.Empty;
        [FirestoreProperty]
        // ค่า Default Role = "admin"
        [DefaultValue("admin")]
        public string Role { get; set; } = "admin";
    }
}