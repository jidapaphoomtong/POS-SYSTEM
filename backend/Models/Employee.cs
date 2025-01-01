using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Employee
    {
        [FirestoreProperty]
        public string firstName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string lastName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string emailName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Role { get; set; } = string.Empty;
    }
}