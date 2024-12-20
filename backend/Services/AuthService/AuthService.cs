using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly FirestoreDB _db;
        private readonly IAuthService _authService;
        public AuthService(FirestoreDB firestore, IAuthService authService){
            _db = firestore;
            _authService = authService;
        }
        
    }
}