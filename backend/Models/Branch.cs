using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Branch
    {
        [FirestoreProperty]
        public int Id { get; set; } 
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Location { get; set; } = string.Empty;
        [FirestoreProperty]
        public string IconUrl { get; set; } = string.Empty;
    }

    public class BranchResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public string Location { get; set; }
    }
}