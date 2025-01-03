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

            var userCollection = _firestoreDb.Collection("users");
            var snapshot = await userCollection.WhereEqualTo("email", email).GetSnapshotAsync();
            return snapshot.Documents.Any();
        }

        public async Task<string> GetNextUserId()
        {
            var settingsCollection = _firestoreDb.Collection("settings");
            var idDocRef = settingsCollection.Document("userIdTracking");
            var idSnapshot = await idDocRef.GetSnapshotAsync();

            int newId = 1;

            // เพิ่มการตรวจสอบการมีอยู่ของเอกสาร
            if (idSnapshot.Exists && idSnapshot.ContainsField("lastUserId"))
            {
                var currentId = idSnapshot.GetValue<int>("lastUserId");
                newId = currentId + 1;
            }

            // อัปเดต/สร้างเอกสารใหม่พร้อมค่าใหม่ของ lastUserId
            await idDocRef.SetAsync(new { lastUserId = newId }, SetOptions.MergeAll);
            return newId.ToString("D3"); // คืนค่า ID 3 หลัก
        }

        public async Task<DocumentSnapshot> GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

            var userCollection = _firestoreDb.Collection("users");
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

            var userCollection = _firestoreDb.Collection("users");
            var userDoc = userCollection.Document(userId); // สร้าง/อ้างอิง Document โดยใช้ userId

            // เพิ่มข้อมูล Role และ Field ต่าง ๆ ลงในเอกสาร
            var data = new
            {
                id = userId,
                firstName = firstName,
                lastName = lastName,
                email = email,
                salt = salt,
                passwordHash = hashedPassword,
                roles = roles
            };

            await userDoc.SetAsync(data); // บันทึกข้อมูลโดยใช้ `SetAsync` (กำหนด ID)
            return userDoc; // ส่งคืน Document Reference
        }

        public async Task<List<Dictionary<string, object>>> GetAllUsers()
        {
            var userCollection = _firestoreDb.Collection("users");
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
            var userDoc = _firestoreDb.Collection("users").Document(userId);
            var snapshot = await userDoc.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Console.WriteLine($"Error: User with ID {userId} not found in Firestore.");
                return false; // ไม่พบเอกสารใน Firestore
            }

            Console.WriteLine($"User found: {System.Text.Json.JsonSerializer.Serialize(snapshot.ToDictionary())}");

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
                Console.WriteLine($"Error: Missing critical fields in existing data for user ID {userId}.");
                return false;
            }

            var passwordHash = existingData["passwordHash"].ToString();
            var salt = existingData["salt"].ToString();

            // เตรียมข้อมูลใหม่สำหรับอัปเดต (รวมกับข้อมูลเก่า)
            var updateData = new
            {
                id = userId,
                firstName = updatedUser.firstName,
                lastName = updatedUser.lastName,
                email = updatedUser.email,
                passwordHash = passwordHash, // ดึงมาจากข้อมูลเดิม
                salt = salt,                 // ดึงมาจากข้อมูลเดิม
                roles = updatedUser.roles.Select(r => new { Id = r.Id, Name = r.Name }).ToList()
            };

            // เขียนข้อมูลกลับไปที่ Firestore
            await userDoc.SetAsync(updateData);

            Console.WriteLine("User updated successfully in Firestore.");
            return true;
        }

        // ลบผู้ใช้ตาม Id
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var userDoc = _firestoreDb.Collection("users").Document(userId);

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
            var userDocs = await _firestoreDb.Collection("users").GetSnapshotAsync();
            foreach (var doc in userDocs.Documents)
            {
                await doc.Reference.DeleteAsync();
            }

            return true;
        }
        
        // Your existing AuthService
        public async Task<DocumentSnapshot> GetUserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            // Access Firestore Collection "users"
            var userCollection = _firestoreDb.Collection("users");
            
            // Query user by userId field
            var snapshot = await userCollection.WhereEqualTo("userId", userId).GetSnapshotAsync();
            
            // Return the corresponding document or null if not found
            return snapshot.Documents.FirstOrDefault();
        }
    }
}