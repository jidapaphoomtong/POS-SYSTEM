using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.EmployeeService
{
    public class EmployeeService : IEmployeeService
    {
        private readonly FirestoreDB _firestoreDb;

        public EmployeeService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
    }
}