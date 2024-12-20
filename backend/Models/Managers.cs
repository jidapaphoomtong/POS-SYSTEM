using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Managers
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}