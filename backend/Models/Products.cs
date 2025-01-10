using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace backend.Models
{
    [FirestoreData]
    public class Products
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty; // ไอดีของสินค้า
        [FirestoreProperty]
        public string ImgUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public string productName{ get; set; } = string.Empty;
        [FirestoreProperty]
        public string description { get; set; } = string.Empty;
        [FirestoreProperty]
        public int stock { get; set; }
        [FirestoreProperty]
        public double price { get; set; }
    }
}