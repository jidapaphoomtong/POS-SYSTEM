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
        public string Id { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Location { get; set; } = string.Empty;
        [FirestoreProperty]
        public string IconUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public string status { get; set; } = string.Empty; // เพิ่ม property นี้
    }

    [FirestoreData]
    public class BranchResponse
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Location { get; set; } = string.Empty;
        [FirestoreProperty]
        public string IconUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public string status { get; set; } = string.Empty; // เพิ่ม property นี้
    }
}