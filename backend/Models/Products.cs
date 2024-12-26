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
        public string Name{ get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}