using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.ManagerService
{
    public class ManagerService : IManagerService
    {
        private readonly FirestoreDB _firestoreDb;

        public ManagerService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
    }
}