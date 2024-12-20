using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Email หรือ PIN ต้องถูกระบุ")]
        public string? Email { get; set; } = string.Empty;

        [RequiredIf(nameof(Email), null, ErrorMessage = "PIN ต้องถูกระบุหากไม่ได้ระบุ Email")]
        public string? PIN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password เป็นข้อมูลที่จำเป็น")]
        public string Password { get; set; } = string.Empty;
    }
}