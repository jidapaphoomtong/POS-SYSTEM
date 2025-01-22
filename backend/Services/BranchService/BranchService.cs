using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;

namespace backend.Services.BranchService
{
    public class BranchService : IBranchService
    {
        private readonly FirestoreDB _firestoreDb;

        public BranchService(FirestoreDB firestoreDb)
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

        public async Task<bool> UpdateBranchStatusAsync(string branchId, string status)
    {
        DocumentReference branchDoc = _firestoreDb.Collection("branches").Document(branchId);

        var branchSnapshot = await branchDoc.GetSnapshotAsync();

        if (!branchSnapshot.Exists)
        {
            return false; // ไม่พบ branch
        }

        try
        {
            // สร้าง Dictionary สำหรับการอัปเดตสถานะ
            var updates = new Dictionary<string, object>
            {
                { "status", status }
            };

            // อัปเดตสถานะ
            await branchDoc.UpdateAsync(updates);
            return true; // อัปเดตสำเร็จ
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating branch status: {ex.Message}");
            return false; // การอัปเดตล้มเหลว
        }
    }

//         public async Task<ServiceResponse<string>> DeleteAllBranches()
//         {
//             try
//             {
//                 // ดึง snapshot ของ branches
//                 var branchesSnapshot = await _firestoreDb.Collection("branches").GetSnapshotAsync();

//                 // หากไม่มี documents ให้ส่งกลับข้อความที่เหมาะสม
//                 if (branchesSnapshot.Count == 0)
//                 {
//                     return ServiceResponse<string>.CreateSuccess("No branches to delete.", "No branches deleted");
//                 }

//                 // ลบแต่ละ Document
//                 foreach (var document in branchesSnapshot.Documents)
//                 {
//                     await document.Reference.DeleteAsync(); // ลบ Document โดยตรง
//                 }

//                 return ServiceResponse<string>.CreateSuccess("All branches deleted successfully!", "All deleted");
//             }
//             catch (Exception ex)
//             {
//                 return ServiceResponse<string>.CreateFailure($"Failed to delete branches: {ex.Message}");
//             }
// }

//         public async Task<ServiceResponse<string>> ResetBranchIdSequence()
//         {
//             try
//             {
//                 var sequenceDoc = _firestoreDb.Collection("config").Document("branch-sequence");
//                 await sequenceDoc.SetAsync(new { counter = 1 }); // รีเซ็ตค่าเป็น 1

//                 return ServiceResponse<string>.CreateSuccess("Branch ID sequence reset successfully!", "Reset done");
//             }
//             catch (Exception ex)
//             {
//                 return ServiceResponse<string>.CreateFailure($"Failed to reset branch ID sequence: {ex.Message}");
//             }
//         }

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
    }
}