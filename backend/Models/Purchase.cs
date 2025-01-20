using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Purchase
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public IList<Products> Products { get; set; }

        [FirestoreProperty]
        public double Total { get; set; }

        [FirestoreProperty]
        public double PaidAmount { get; set; }

        [FirestoreProperty]
        public double Change { get; set; }

        [FirestoreProperty]
        public DateTime Date { get; set; }

        [FirestoreProperty]
        public string Seller { get; set; } // ฟิลด์ที่ใช้สำหรับชื่อคนขาย
    }
}