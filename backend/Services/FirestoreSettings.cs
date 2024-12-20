using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class FirestoreSettings
    {
        public string ProjectId { get; set; }
        public string DatabaseId { get; set; }
        public string PrivateKey { get; set; }
        public string ClientEmail { get; set; }
    }
}