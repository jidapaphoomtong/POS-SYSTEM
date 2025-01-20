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
        public decimal Total { get; set; }

        [FirestoreProperty]
        public decimal PaidAmount { get; set; }

        [FirestoreProperty]
        public decimal Change { get; set; }

        [FirestoreProperty]
        public DateTime Date { get; set; }

        [FirestoreProperty]
        public string SellerId { get; set; } // เพิ่มฟิลด์สำหรับ sellerId
    }
}