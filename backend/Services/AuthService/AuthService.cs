using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace backend.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly FirestoreDB _firestoreDb;

        public AuthService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb ?? throw new ArgumentNullException(nameof(firestoreDb));
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

            var userCollection = _firestoreDb.Collection(FirestoreCollections.Users);
            var snapshot = await userCollection.WhereEqualTo("email", email).GetSnapshotAsync();
            return snapshot.Documents.Any();
        }

        public async Task<string> GetNextUserId()
        {
            var sequenceDoc = _firestoreDb.Collection(FirestoreCollections.Config).Document("sequence");
            var snapshot = await sequenceDoc.GetSnapshotAsync();

            int counter = 1;

            if (snapshot.Exists && snapshot.TryGetValue<int>("counter", out var currentCounter))
            {
                counter = currentCounter;
            }

            // บันทึกลำดับใหม่ (เพิ่ม 1)
            await sequenceDoc.SetAsync(new { counter = counter + 1 });

            // คืนค่า ID ในรูปแบบ "001", "002"
            return counter.ToString("D3");
        }

        public async Task<DocumentSnapshot> GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

            var userCollection = _firestoreDb.Collection(FirestoreCollections.Users);
            var snapshot = await userCollection.WhereEqualTo("email", email).GetSnapshotAsync();
            return snapshot.Documents.FirstOrDefault();
        }

        public async Task<DocumentReference> RegisterUserAsync(
            string userId,
            string firstName,
            string lastName,
            string email,
            string salt,
            string hashedPassword,
            IList<Role> roles)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(hashedPassword))
                throw new ArgumentException("UserId, email, and password hash cannot be null or empty.");

            var userCollection = _firestoreDb.Collection(FirestoreCollections.Users);
            var userDoc = userCollection.Document(userId); // สร้าง/อ้างอิง Document โดยใช้ userId

            // เพิ่มข้อมูล Role และ Field ต่าง ๆ ลงในเอกสาร
            var defaultRole = new List<Role>
            {
                new Role { Id = 1, Name = "Employee" } // Default Role เป็น Employee
            };

            // ภายใน RegisterUserAsync
            var data = new
            {
                id = userId,
                firstName = firstName,
                lastName = lastName,
                email = email,
                salt = salt,
                passwordHash = hashedPassword,
                roles = roles.Any() ? roles : defaultRole // ถ้าไม่มี Role จะใช้ Default
            };
            await userDoc.SetAsync(data); // บันทึกข้อมูลโดยใช้ `SetAsync` (กำหนด ID)
            return userDoc; // ส่งคืน Document Reference
        }

        public async Task<List<Dictionary<string, object>>> GetAllUsers()
        {
            var userCollection = _firestoreDb.Collection(FirestoreCollections.Users);
            var snapshot = await userCollection.GetSnapshotAsync();

            return snapshot.Documents.Select(doc =>
            {
                var user = doc.ToDictionary();
                // if (!user.ContainsKey("role"))
                // {
                //     user["role"] = "Admin"; // Default role
                // }
                return user;
            }).ToList();
        }

        // อัปเดตข้อมูลผู้ใช้
        public async Task<bool> UpdateUserAsync(string userId, User updatedUser)
        {
            Console.WriteLine($"Attempting to update user with ID: {userId}");

            // อ้างอิง Document ใน Firestore
            var userDoc = _firestoreDb.Collection(FirestoreCollections.Users).Document(userId);
            var snapshot = await userDoc.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false; // ไม่พบเอกสารใน Firestore
            }


            // Role Validation
            var allowedRoles = new List<string> { "admin", "manager", "employee" };
            if (updatedUser.roles.Any(r => !allowedRoles.Contains(r.Name.ToLower())))
            {
                throw new ArgumentException("One or more roles are invalid.");
            }

            // ดึงข้อมูลเดิมจาก Firestore
            var existingData = snapshot.ToDictionary();

            // รักษาข้อมูลเดิมที่สำคัญ เช่น passwordHash และ salt
            if (!existingData.ContainsKey("passwordHash") || !existingData.ContainsKey("salt"))
            {
                return false;
            }

            var passwordHash = existingData["passwordHash"].ToString();
            var salt = existingData["salt"].ToString();

            // เตรียมข้อมูลใหม่สำหรับอัปเดต
            var updateData = new Dictionary<string, object>
            {
                { "id", userId },
                { "firstName", string.IsNullOrWhiteSpace(updatedUser.firstName) ? existingData["firstName"] : updatedUser.firstName },
                { "lastName", string.IsNullOrWhiteSpace(updatedUser.lastName) ? existingData["lastName"] : updatedUser.lastName },
                { "email", string.IsNullOrWhiteSpace(updatedUser.email) ? existingData["email"] : updatedUser.email },
                { "passwordHash", passwordHash }, // ดึงมาจากข้อมูลเดิม
                { "salt", salt },                 // ดึงมาจากข้อมูลเดิม
                { "roles", updatedUser.roles.Select(r => new { Id = r.Id, Name = r.Name }).ToList() }
            };

            // อัปเดตข้อมูลใน Firestore
            await userDoc.SetAsync(updateData, SetOptions.MergeAll); // MergeAll จะอัปเดตเฉพาะข้อมูลที่ส่งมา

            return true;
        }

        // ลบผู้ใช้ตาม Id
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var userDoc = _firestoreDb.Collection(FirestoreCollections.Users).Document(userId);

            var snapshot = await userDoc.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                return false; // ไม่พบ User
            }

            await userDoc.DeleteAsync();
            return true;
        }

        // ลบผู้ใช้ทั้งหมด
        public async Task<bool> DeleteAllUsersAsync()
        {
            var userDocs = await _firestoreDb.Collection(FirestoreCollections.Users).GetSnapshotAsync();
            foreach (var doc in userDocs.Documents)
            {
                await doc.Reference.DeleteAsync();
            }

            return true;
        }
        
        // Your existing AuthService
        public async Task<DocumentSnapshot> GetUserById(string userId)
        {
            var userRef = _firestoreDb.Collection(FirestoreCollections.Users).Document(userId);
            var snapshot = await userRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot : null;
        }
        public async Task ResetUserIdSequence()
        {
            var sequenceDoc = _firestoreDb.Collection("config").Document("sequence");
            await sequenceDoc.SetAsync(new { counter = 1 }); // ตั้งค่าเริ่มต้นใหม่เป็น 001
        }

    }
}