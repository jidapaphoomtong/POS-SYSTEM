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
        public string Id { get; set; }
        [FirestoreProperty]
        public string productName{ get; set; } = string.Empty;
        [FirestoreProperty]
        public int quantity { get; set; }
        [FirestoreProperty]
        public double price { get; set; }
    }
}