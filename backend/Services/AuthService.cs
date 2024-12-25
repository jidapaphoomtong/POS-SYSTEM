using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class AuthService
    {
        private readonly FirestoreDB _firestoreDb;

        public AuthService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

    }
}