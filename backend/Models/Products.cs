using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Title{ get; set; } = string.Empty;
        public int Quantity { get; set; } = 20;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}