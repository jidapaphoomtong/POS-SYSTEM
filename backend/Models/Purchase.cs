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
        public string Id { get; set; } = string.Empty;

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
        
        [FirestoreProperty]
        public string PaymentMethod { get; set; } = string.Empty; 
    }

    public class SalesSummaryDto
    {
        public double TotalSales { get; set; }
        public int TotalTransactions { get; set; }
        public List<DailySalesDto> DailySales { get; set; }
    }

    public class DailySalesDto
    {
        public string Date { get; set; }              // วันที่ในรูปแบบ "yyyy-MM-dd"
        public double Amount { get; set; }             // ยอดขายรวมในแต่ละวัน
        public int TransactionCount { get; set; }      // จำนวนบิลในแต่ละวัน
        public double AveragePerTransaction { get; set; } // เฉลี่ยยอดขายต่อบิล
    }
}