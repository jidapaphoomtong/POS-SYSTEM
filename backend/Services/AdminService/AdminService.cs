using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;

namespace backend.Services.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly FirestoreDB _firestoreDb;

        public AdminService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        public string HashPassword(string password, string salt)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
            if (string.IsNullOrWhiteSpace(salt)) throw new ArgumentException("Salt cannot be null or whitespace.", nameof(salt));

            var saltBytes = Convert.FromBase64String(salt);
            var hashed = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32
            );
            return Convert.ToBase64String(hashed);
        }

        public bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            if (string.IsNullOrWhiteSpace(enteredPassword)) throw new ArgumentException("Entered password cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt))
                return false;

            var hashOfEnteredPassword = HashPassword(enteredPassword, storedSalt);
            return hashOfEnteredPassword == storedHash;
        }

        public async Task<bool> IsEmailRegistered(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

            var userCollection = _firestoreDb.Collection("employees");
            var snapshot = await userCollection.WhereEqualTo("email", email).GetSnapshotAsync();
            return snapshot.Documents.Any();
        }
        
        public async Task<string> GetNextId(string sequenceName)
        {
            try
            {
                var sequenceDoc = _firestoreDb.Collection("config").Document(sequenceName);
                var snapshot = await sequenceDoc.GetSnapshotAsync();

                int counter = 1;

                if (snapshot.Exists && snapshot.TryGetValue<int>("counter", out var currentCounter))
                {
                    counter = currentCounter;
                }

                // Increment ลำดับ
                await sequenceDoc.SetAsync(new { counter = counter + 1 });

                // คืนค่า ID ในรูปแบบ "001", "002", "003"
                return counter.ToString("D3");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate ID for {sequenceName}: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> AddBranch(Branch branch)
        {
            try
            {
                // สร้างไอดีแบบกำหนดเอง
                string branchId = await GetNextId("branch-sequence");

                // สร้าง Document ID โดยใช้ไอดีที่กำหนดเอง
                DocumentReference branchDoc = _firestoreDb.Collection("branches").Document(branchId);

                // เพิ่มไอดีเข้าไปใน Branch Object
                branch.Id = branchId; // เก็บไอดีในฟิลด์ "Id" แบบ string
                await branchDoc.SetAsync(branch); // บันทึกข้อมูลใน Firestore

                return ServiceResponse<string>.CreateSuccess(branchId, "Branch added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add branch: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> AddEmployee(string branchId, Employee employee, string salt, string hashedPassword)
        {
            try
            {
                // ตรวจสอบค่าที่ส่งเข้ามา
                if (string.IsNullOrWhiteSpace(branchId) || employee == null || 
                    string.IsNullOrWhiteSpace(employee.email) || 
                    string.IsNullOrWhiteSpace(hashedPassword) || 
                    string.IsNullOrWhiteSpace(salt))
                {
                    throw new ArgumentException("BranchId, Employee object, email, password hash, and salt cannot be null or empty.");
                }

                // ตรวจสอบว่า Branch มีอยู่ใน Firestore หรือไม่
                DocumentReference branchDoc = _firestoreDb.Collection("branches").Document(branchId);
                DocumentSnapshot branchSnapshot = await branchDoc.GetSnapshotAsync();
                if (!branchSnapshot.Exists)
                {
                    return ServiceResponse<string>.CreateFailure("Branch does not exist.");
                }

                // ตรวจสอบว่า Email ซ้ำหรือไม่
                if (await IsEmailRegistered(employee.email))
                {
                    return ServiceResponse<string>.CreateFailure("Email is already registered.");
                }

                // สร้าง Employee ID ใหม่
                string employeeId = await GetNextId($"employee-sequence-{branchId}");

                // กำหนด Employee ID และ Branch ID
                employee.Id = employeeId;
                employee.branchId = branchId;

                // ตั้งค่า Role ค่าเริ่มต้น
                employee.role = employee.role?.Count > 0 ? employee.role : new List<Role>
                {
                    new Role { Id = 1, Name = "Employee" }
                };

                // กำหนดค่า Hashed Password และ Salt
                employee.password = hashedPassword;
                employee.Salt = salt;

                // เพิ่ม Timestamp
                employee.CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow);
                employee.UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow);

                // อ้างถึง Document ID
                DocumentReference employeeDoc = branchDoc.Collection("employees").Document(employeeId);

                // บันทึก Employee ลงใน Firestore
                await employeeDoc.SetAsync(employee);

                return ServiceResponse<string>.CreateSuccess(employeeId, "Employee added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"An error occurred while adding the employee: {ex.Message}");
            }
        }



        public async Task<ServiceResponse<string>> AddProduct(string branchId, Products product)
        {
            try
            {
                // สร้าง Product ID ใหม่
                string productId = await GetNextId($"product-sequence-{branchId}"); // ลำดับเฉพาะต่อ Branch

                // อ้างถึง Document ID
                DocumentReference productDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("products")
                    .Document(productId);

                // เพิ่ม Product ID เข้าไปใน Object ก่อนบันทึก
                product.Id = productId;

                // บันทึก Product ลงใน Firestore
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
            try {
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

        public async Task<ServiceResponse<string>> UpdateBranch(string branchId, Branch branch)
        {
            try
            {
                DocumentReference branchDoc = _firestoreDb.Collection("branches").Document(branchId);
                
                // ตรวจสอบว่า Document มีอยู่
                var branchDocSnapshot = await branchDoc.GetSnapshotAsync();
                if (!branchDocSnapshot.Exists)
                {
                    return ServiceResponse<string>.CreateFailure("Branch document does not exist.");
                }

                // แปลง branch เป็น dictionary สำหรับอัปเดต Firestore
                var updatedData = new Dictionary<string, object>
                {
                    { "Name", branch.Name },
                    { "Location", branch.Location },
                    { "IconUrl", branch.IconUrl }
                };

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
                var updateData = new Dictionary<string, object>();

                // ตรวจสอบข้อมูลที่จำเป็น
                if (!string.IsNullOrWhiteSpace(updatedEmployee.firstName))
                {
                    updateData["firstName"] = updatedEmployee.firstName;
                }

                if (!string.IsNullOrWhiteSpace(updatedEmployee.lastName))
                {
                    updateData["lastName"] = updatedEmployee.lastName;
                }

                if (!string.IsNullOrWhiteSpace(updatedEmployee.email))
                {
                    updateData["email"] = updatedEmployee.email;
                }

                if (!string.IsNullOrWhiteSpace(updatedEmployee.password))
                {
                    updateData["password"] = updatedEmployee.password;
                }

                // อัปเดตข้อมูล Role ถ้ามีการเปลี่ยนแปลง
                if (updatedEmployee.role != null && updatedEmployee.role.Any())
                {
                    updateData["role"] = updatedEmployee.role;
                }

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
                    { "stock", updatedProduct.stock }
                });

                return ServiceResponse<string>.CreateSuccess(productId, "Product updated successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to update product: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<BranchResponse>> GetBranchById(string branchId)
        {
            try
            {
                DocumentReference branchDoc = _firestoreDb.Collection("branches").Document(branchId);
                Console.WriteLine($"Fetching branch with ID: {branchId}");

                // ตรวจสอบว่า Document มีอยู่
                var branchDocSnapshot = await branchDoc.GetSnapshotAsync();
                if (!branchDocSnapshot.Exists)
                {
                    Console.WriteLine($"Branch with ID {branchId} does not exist."); // Logging
                    return ServiceResponse<BranchResponse>.CreateFailure("Branch document does not exist.");
                }

                // แปลงข้อมูลจาก Firestore เป็น BranchResponse
                var branch = branchDocSnapshot.ConvertTo<BranchResponse>();
                Console.WriteLine($"Branch fetched: {JsonConvert.SerializeObject(branch)}");

                return ServiceResponse<BranchResponse>.CreateSuccess(branch, "Branch fetched successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching branch: {ex.Message}"); // Logging error
                return ServiceResponse<BranchResponse>.CreateFailure($"Failed to fetch branch: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> DeleteAllBranches()
        {
            try
            {
                // ดึง snapshot ของ branches
                var branchesSnapshot = await _firestoreDb.Collection("branches").GetSnapshotAsync();

                // หากไม่มี documents ให้ส่งกลับข้อความที่เหมาะสม
                if (branchesSnapshot.Count == 0)
                {
                    return ServiceResponse<string>.CreateSuccess("No branches to delete.", "No branches deleted");
                }

                // ลบแต่ละ Document
                foreach (var document in branchesSnapshot.Documents)
                {
                    await document.Reference.DeleteAsync(); // ลบ Document โดยตรง
                }

                return ServiceResponse<string>.CreateSuccess("All branches deleted successfully!", "All deleted");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to delete branches: {ex.Message}");
            }
}

        public async Task<ServiceResponse<string>> ResetBranchIdSequence()
        {
            try
            {
                var sequenceDoc = _firestoreDb.Collection("config").Document("branch-sequence");
                await sequenceDoc.SetAsync(new { counter = 1 }); // รีเซ็ตค่าเป็น 1

                return ServiceResponse<string>.CreateSuccess("Branch ID sequence reset successfully!", "Reset done");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to reset branch ID sequence: {ex.Message}");
            }
        }

        public async Task<Employee?> GetEmployeeByEmail(string branchId, string email)
        {
            var collectionReference = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("employees")
                    .WhereEqualTo("email", email);
            
            Console.WriteLine($"Querying for employee with email: {email}"); // Added log

            var query = collectionReference.WhereEqualTo("email", email);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count == 0)
            {
                Console.WriteLine($"No employee found with email: {email}"); // Added log
                return null;
            }

            var document = snapshot.Documents[0];
            var employee = document.ConvertTo<Employee>();
            employee.Id = document.Id;

            return employee;
        }
    }
}