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
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string BranchId { get; set; }
        public string ProductId { get; set; }
    }
}