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
        
        public async Task<string> GenerateSequentialId(string collectionName)
        {
            CollectionReference collection = _firestoreDb.Collection(collectionName);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();

            // สร้าง Id ต่อเนื่อง (แบบ 001, 002, 003)
            var maxId = snapshot.Documents.Select(doc => int.Parse(doc.Id)).DefaultIfEmpty(0).Max();
            return (maxId + 1).ToString("D3"); // Format เป็นแบบ 3 หลัก (001)
        }

        public async Task<ServiceResponse<string>> AddBranch(Branch branch)
        {
            try
            {
                string branchId = await GenerateSequentialId("branches"); // Generate ID
                DocumentReference branchDoc = _firestoreDb.Collection("branches").Document(branchId);
                await branchDoc.SetAsync(branch);
                return ServiceResponse<string>.CreateSuccess(branchId, "Branch added successfully!");
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
                string employeeId = await GenerateSequentialId($"branches/{branchId}/employees"); // Generate ID
                DocumentReference employeeDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("employees")
                    .Document(employeeId);
                await employeeDoc.SetAsync(employee);
                return ServiceResponse<string>.CreateSuccess(employeeId, "Employee added successfully!");
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
                string productId = await GenerateSequentialId($"branches/{branchId}/products"); // Generate ID
                DocumentReference productDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);
                await productDoc.SetAsync(product);
                return ServiceResponse<string>.CreateSuccess(productId, "Product added successfully!");
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