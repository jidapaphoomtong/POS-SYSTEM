using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class FirestoreSettings
    {
        public string PROJECT_ID { get; set; } = string.Empty;
        public string DATABASE_ID { get; set; } = string.Empty;
        public string PRIVATE_KEY { get; set; } = string.Empty;
        public string CLIENT_EMAIL { get; set; } = string.Empty;
    }

    
}