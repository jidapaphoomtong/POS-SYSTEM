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
        public async Task<ServiceResponse<string>> AddBranch(Branch branch)
        {
            try
            {
                DocumentReference newBranch = await _firestoreDb.Collection("branches").AddAsync(branch);
                return ServiceResponse<string>.CreateSuccess(newBranch.Id, "Branch added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add branch: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> AddEmployee(string branchId, Employee employee)
        {
            try
            {
                var employeeDoc = await _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("employees").AddAsync(employee);
                return ServiceResponse<string>.CreateSuccess(employeeDoc.Id, "Employee added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add employee: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> AddProduct(string branchId, Products product)
        {
            try
            {
                var productDoc = await _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("products").AddAsync(product);
                return ServiceResponse<string>.CreateSuccess(productDoc.Id, "Product added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> DeleteBranch(string branchId)
        {
            try
            {
                DocumentReference branch = _firestoreDb
                    .Collection("branches")
                    .Document(branchId);

                await branch.DeleteAsync();
                return ServiceResponse<string>.CreateSuccess(branchId, "Branch deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete branch: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> DeleteEmployee(string branchId, string employeeId)
        {
            try
            {
                DocumentReference employeeDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("employees")
                    .Document(employeeId);

                await employeeDoc.DeleteAsync();

                return ServiceResponse<string>.CreateSuccess(employeeId, "Employee deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete employee: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> DeleteProduct(string branchId, string productId)
        {
            try
            {
                DocumentReference productDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                await productDoc.DeleteAsync();

                return ServiceResponse<string>.CreateSuccess(productId, "Product deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<BranchResponse>>> GetBranches()
        {
            try
            {
                CollectionReference branches = _firestoreDb.Collection("branches");
                QuerySnapshot snapshot = await branches.GetSnapshotAsync();

                var branchesList = snapshot.Documents.Select(doc => new BranchResponse
                {
                    Id = doc.Id,
                    Name = doc.GetValue<string>("Name"),
                    IconUrl = doc.GetValue<string>("IconUrl"),
                    Location = doc.GetValue<string>("Location")
                }).ToList();

                return ServiceResponse<List<BranchResponse>>.CreateSuccess(branchesList, "Branches fetched successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<BranchResponse>>.CreateFailure($"Failed to fetch branches: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<object>>> GetEmployees(string branchId)
        {
            try
            {
                CollectionReference employees = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("employees");

                QuerySnapshot snapshot = await employees.GetSnapshotAsync();

                var employeesList = snapshot.Documents.Select(doc => new
                {
                    Id = doc.Id,
                    Data = doc.ToDictionary()
                }).Cast<object>().ToList();

                return ServiceResponse<List<object>>.CreateSuccess(employeesList, "Employees fetched successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<object>>.CreateFailure($"Failed to fetch employees: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<object>>> GetProducts(string branchId)
        {
            try
            {
                CollectionReference products = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("products");

                QuerySnapshot snapshot = await products.GetSnapshotAsync();

                var productsList = snapshot.Documents.Select(doc => new
                {
                    Id = doc.Id,
                    Data = doc.ToDictionary()
                }).Cast<object>().ToList();

                return ServiceResponse<List<object>>.CreateSuccess(productsList, "Products fetched successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<object>>.CreateFailure($"Failed to fetch products: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> UpdateBranch(string branchId, Dictionary<string, object> updatedData)
        {
            try
            {
                DocumentReference branchDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId);

                await branchDoc.UpdateAsync(updatedData);

                return ServiceResponse<string>.CreateSuccess(branchId, "Branch updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update branch: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> UpdateEmployee(string branchId, string employeeId, Employee updatedEmployee)
        {
            try
            {
                DocumentReference employeeDoc = _firestoreDb
                    .Collection("branches")
                    .Document(branchId)
                    .Collection("employees")
                    .Document(employeeId);

                // สร้างข้อมูลใหม่ที่จะอัปเดต
                var updateData = new Dictionary<string, object>
                {
                    { "firstName", updatedEmployee.firstName },
                    { "lastName", updatedEmployee.lastName },
                    { "emailName", updatedEmployee.emailName },
                    { "Role", updatedEmployee.role } // เพิ่ม Role ในการอัปเดต
                };

                // อัปเดตข้อมูลใน Firestore
                await employeeDoc.UpdateAsync(updateData);

                return ServiceResponse<string>.CreateSuccess(employeeId, "Employee updated successfully!");
            }
            catch (Exception ex)
            {
                // ส่งข้อผิดพลาดในกรณีที่ล้มเหลว
                return ServiceResponse<string>.CreateFailure($"Failed to update employee: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> UpdateProduct(string branchId, string productId, Products updatedProduct)
        {
            try
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

                return ServiceResponse<string>.CreateSuccess(productId, "Product updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update product: {ex.Message}");
            }
        }

    }
}