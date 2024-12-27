using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;

namespace backend.Services.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly FirestoreDB _firestoreDb;

        public AdminService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        
        //admin only
        public async Task<string> AddBranch(Branch branch)
        {
            CollectionReference branches = _firestoreDb.Collection("branches");
            DocumentReference newBranch = await branches.AddAsync(branch);
            return newBranch.Id;
        }

        //admin and manager
        public async Task<string> AddEmployee(string branchId, Employee employee)
        {
            CollectionReference employees = _firestoreDb
                .Collection("branches")
                .Document(branchId)
                .Collection("employees");
            DocumentReference newEmployee = await employees.AddAsync(employee);
            return newEmployee.Id;
        }
        
        //manager
        public async Task<string> AddProduct(string branchId, Products product)
        {
            CollectionReference products = _firestoreDb
                .Collection("branches")
                .Document(branchId)
                .Collection("products");
            DocumentReference newProduct = await products.AddAsync(product);
            return newProduct.Id;
        }

        public async Task DeleteBranch(string branchId)
        {
            DocumentReference branchDoc = _firestoreDb
                .Collection("branches")
                .Document(branchId);
            await branchDoc.DeleteAsync();
        }

        public async Task DeleteEmployee(string branchId, string employeeId)
        {
            DocumentReference employeeDoc = _firestoreDb
                .Collection("branches")
                .Document(branchId)
                .Collection("employees")
                .Document(employeeId);
            await employeeDoc.DeleteAsync();
        }

        public async Task DeleteProduct(string branchId, string productId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<object>> GetBranches()
        {
            CollectionReference branches = _firestoreDb.Collection("branches");
            QuerySnapshot snapshot = await branches.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                Data = doc.ToDictionary()
            }).Cast<object>().ToList();
        }

        public async Task<List<object>> GetEmployees(string branchId)
        {
            CollectionReference employees = _firestoreDb
                .Collection("branches")
                .Document(branchId)
                .Collection("employees");
            QuerySnapshot snapshot = await employees.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                Data = doc.ToDictionary()
            }).Cast<object>().ToList();
        }

        public async Task<List<object>> GetProducts(string branchId)
        {
            CollectionReference products = _firestoreDb
                .Collection("branches")
                .Document(branchId)
                .Collection("products");
            QuerySnapshot snapshot = await products.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                Data = doc.ToDictionary()
            }).Cast<object>().ToList();
        }

        public async Task UpdateBranch(string branchId, Dictionary<string, object> updatedData)
        {
            DocumentReference branchDoc = _firestoreDb
                .Collection("branches")
                .Document(branchId);
            await branchDoc.UpdateAsync(updatedData);
        }

        public async Task UpdateEmployee(string branchId, string employeeId, Employee updatedEmployee)
        {
            DocumentReference employeeDoc = _firestoreDb
                .Collection("branches")
                .Document(branchId)
                .Collection("employees")
                .Document(employeeId);
            await employeeDoc.UpdateAsync(new Dictionary<string, object>
            {
                { "firstName", updatedEmployee.firstName },
                { "lastName", updatedEmployee.lastName },
                { "emailName", updatedEmployee.emailName },
                { "Role", updatedEmployee.Role }
            });
        }

        public async Task UpdateProduct(string branchId, string productId, Products updatedProduct)
        {
            DocumentReference productDoc = _firestoreDb
                .Collection("branches")
                .Document(branchId)
                .Collection("products")
                .Document(productId);
            await productDoc.UpdateAsync(new Dictionary<string, object>
            {
                { "productName", updatedProduct.productName },
                { "price", updatedProduct.price },
                { "quantity", updatedProduct.quantity }
            });
        }
    }
}