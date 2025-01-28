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
    public class User
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;
        [FirestoreProperty]
        public string firstName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string lastName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string email { get; set; } = string.Empty;
        [FirestoreProperty]
        public string password { get; set; } = string.Empty;
        // [FirestoreProperty]
        // public string RefreshToken { get; set; }
        [FirestoreProperty]
        public IList<Role> roles { get; set; } = new List<Role>();
    }
}
