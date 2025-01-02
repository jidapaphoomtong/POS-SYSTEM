using System;
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
            var hashOfEnteredPassword = HashPassword(enteredPassword, storedSalt);
            return hashOfEnteredPassword == storedHash;
        }

        public async Task<bool> IsEmailRegistered(string email)
        {
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
            if (idSnapshot.Exists)
            {
                var currentId = idSnapshot.GetValue<int>("lastUserId");
                newId = currentId + 1;
            }

            await idDocRef.SetAsync(new { lastUserId = newId });
            return newId.ToString("D3");
        }

        public async Task<DocumentSnapshot> GetUserByEmail(string email)
        {
            var userCollection = _firestoreDb.Collection("users");
            var snapshot = await userCollection.WhereEqualTo("email", email).GetSnapshotAsync();
            return snapshot.Documents.FirstOrDefault();
        }

        public async Task<DocumentReference> RegisterUserAsync(string userId, string firstName, string lastName, string email, string salt, string hashedPassword)
        {
            var userCollection = _firestoreDb.Collection("users");

            return await userCollection.AddAsync(new
            {
                id = userId,
                firstName = firstName,
                lastName = lastName,
                email = email,
                salt = salt,
                passwordHash = hashedPassword
            });
        }

        public async Task<List<Dictionary<string, object>>> GetAllUsers()
        {
            var userCollection = _firestoreDb.Collection("users");
            var snapshot = await userCollection.GetSnapshotAsync();
            
            var users = snapshot.Documents.Select(doc =>
            {
                var user = doc.ToDictionary();

                if (!user.ContainsKey("role"))
                {
                    user["role"] = "Admin"; // Default role
                }

                return user;
            }).ToList();

            return users;
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUser updateUser)
        {
            var userRef = _firestoreDb.Collection("users").Document(id);
            var snapshot = await userRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            var updateData = new Dictionary<string, object>
            {
                { "firstname", updateUser.FirstName },
                { "lastname", updateUser.LastName },
                { "email", updateUser.Email },
                { "role", updateUser.Role }
            };

            await userRef.UpdateAsync(updateData);
            return true;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var userRef = _firestoreDb.Collection("users").Document(id);
            var snapshot = await userRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await userRef.DeleteAsync();
            return true;
        }

        public async Task<bool> DeleteAllUsersAsync()
        {
            var usersRef = _firestoreDb.Collection("users");
            var snapshot = await usersRef.GetSnapshotAsync();

            // ลบเอกสารทั้งหมดในคอลเลกชัน users
            foreach (var doc in snapshot.Documents)
            {
                await doc.Reference.DeleteAsync();
            }

            // รีเซ็ต lastUserId เป็น 0
            var settingsCollection = _firestoreDb.Collection("settings");
            var idDocRef = settingsCollection.Document("userIdTracking");
            await idDocRef.SetAsync(new { lastUserId = 0 });

            return true;
        }
    }
}