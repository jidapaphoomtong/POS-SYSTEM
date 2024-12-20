using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class UpdateModel
        {
            public string Name { get; set; }
            public DateTime? CreatedAt { get; set; } // ใช้ Nullable Time หากไม่ต้องการบังคับ
            public string Description { get; set; }
        }
}