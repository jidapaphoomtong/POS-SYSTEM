using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Purchase
    {
        public string Id {get; set;}
        public IList<Products> Products { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Change { get; set; }
        public DateTime Date { get; set; }
    }
}