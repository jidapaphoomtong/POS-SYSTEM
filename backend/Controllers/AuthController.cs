using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FirestoreDB _firestoreDb;

        public AuthController(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // สร้างค่า Salt
        public static string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        // ฟังก์ชันสำหรับ Hash รหัสผ่านโดยใช้ Salt
        public static string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashed = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32);
            return Convert.ToBase64String(hashed);
        }

        // ฟังก์ชันสำหรับตรวจสอบรหัสผ่าน
        public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            var hashOfEnteredPassword = HashPassword(enteredPassword, storedSalt);
            return hashOfEnteredPassword == storedHash;
        }

        //ตรวจสอบการเข้าระบบ
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register userRegister)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                // ตรวจสอบว่าอีเมลนี้มีอยู่แล้วใน Firestore หรือไม่
                var userCollection = _firestoreDb.Collection("users");
                var snapshot = await userCollection.WhereEqualTo("email", userRegister.Email).GetSnapshotAsync();

                if (snapshot.Documents.Any())
                {
                    return Conflict(new { Success = false, Message = "Email already registered." });
                }

                // ---- เริ่มส่วนการจัดการ Id อัตโนมัติ ----
                var settingsCollection = _firestoreDb.Collection("settings");
                var idDocRef = settingsCollection.Document("userIdTracking");
                var idSnapshot = await idDocRef.GetSnapshotAsync();

                int newId;
                if (idSnapshot.Exists)
                {
                    var currentId = idSnapshot.GetValue<int>("lastUserId");
                    newId = currentId + 1;
                }
                else
                {
                    newId = 1;
                }

                await idDocRef.SetAsync(new { lastUserId = newId });

                // สร้าง Salt และ Hash รหัสผ่าน
                var salt = GenerateSalt();
                var hashedPassword = HashPassword(userRegister.Password, salt);

                // เพิ่มผู้ใช้ใหม่
                var newUser = await userCollection.AddAsync(new
                {
                    id = newId,
                    fullName = userRegister.FullName,
                    email = userRegister.Email,
                    salt = salt, // บันทึก Salt
                    passwordHash = hashedPassword // บันทึก Hash รหัสผ่าน
                });

                return Ok(new { Success = true, Message = "User registered successfully!", UserId = newId, DocumentId = newUser.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred during registration. " + ex.Message });
            }
        }

        //ตรวจสอบการเข้าสู่ระบบ
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userCollection = _firestoreDb.Collection("users");
            var snapshot = await userCollection.WhereEqualTo("email", userLogin.Email).GetSnapshotAsync();

            if (!snapshot.Documents.Any())
            {
                return NotFound(new { Success = false, Message = "User not found" });
            }

            var user = snapshot.Documents.First();
            var userData = user.ToDictionary();

            // ตรวจสอบรหัสผ่าน
            var salt = (string)userData["salt"]; // ดึง Salt จากฐานข้อมูล
            var passwordHash = (string)userData["passwordHash"]; // ดึง Hash จากฐานข้อมูล

            if (!VerifyPassword(userLogin.Password, passwordHash, salt))
            {
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }

            // สร้าง JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("wrZzjoEgLiypg53ojlxG"); // ใช้ Key เดียวกับใน Program.cs

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id), // รหัสของผู้ใช้ในระบบ
                    new Claim(ClaimTypes.Email, (string)userData["email"]) // อีเมลของผู้ใช้
                }),
                Expires = DateTime.UtcNow.AddDays(30), // ตั้งค่า Token ให้หมดอายุใน 1 ชั่วโมง
                Issuer = "localhost",
                Audience = "localhost",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new 
            { 
                Success = true, 
                Message = "Login successful!", 
                Token = tokenString 
            });
        }

        [HttpGet("protected-data")]
        [Authorize]
        public IActionResult GetProtectedData()
        {
            return Ok(new
            {
                Success = true,
                Data = "This is protected data that requires valid JWT token!"
            });
        }

        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                // สร้างอ้างอิงถึง collection "users"
                var userCollection = _firestoreDb.Collection("users");

                // ดึงข้อมูลจาก collection
                var snapshot = await userCollection.GetSnapshotAsync();

                // ตรวจสอบว่ามีข้อมูลหรือไม่
                if (!snapshot.Documents.Any())
                {
                    return NotFound(new { Success = false, Message = "No users found in Firestore." });
                }

                // แปลง snapshot เป็นข้อมูลที่ต้องการส่งออก (List ของ users)
                var users = snapshot.Documents
                    .Select(doc => new 
                    { 
                        Id = doc.Id, 
                        Fields = doc.ToDictionary() 
                    })
                    .ToList();

                // ส่งข้อมูลสำเร็จพร้อมรายละเอียด
                return Ok(new { Success = true, Documents = users });
            }
            catch (Exception ex)
            {
                // ในกรณีที่มีปัญหาอื่นๆ
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("update/{documentId}")]
        public async Task<IActionResult> UpdateUser(string documentId, [FromBody] UpdateUser updateUser)
        {
            try
            {
                var docRef = _firestoreDb.Collection("users").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                var updateData = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(updateUser.FullName))
                    updateData["fullName"] = updateUser.FullName;

                if (!string.IsNullOrWhiteSpace(updateUser.Email))
                    updateData["email"] = updateUser.Email;

                if (!string.IsNullOrWhiteSpace(updateUser.Password))
                {
                    var newSalt = GenerateSalt();
                    var newHashedPassword = HashPassword(updateUser.Password, newSalt);
                    updateData["salt"] = newSalt;
                    updateData["passwordHash"] = newHashedPassword;
                }

                if (updateData.Count == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "No valid fields specified for update."
                    });
                }

                await docRef.UpdateAsync(updateData);

                return Ok(new
                {
                    Success = true,
                    Message = "User updated successfully!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred while updating the user: {ex.Message}"
                });
            }
        }

        [HttpDelete("delete/{documentId}")]
        public async Task<IActionResult> DeleteUser(string documentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return BadRequest(new { Success = false, Message = "Document ID is required for deleting a user." });
                }

                // อ้างถึงเอกสารใน Firestore ด้วย documentId
                var docRef = _firestoreDb.Collection("users").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                // ตรวจสอบว่ามีเอกสารที่ตรงกับ documentId หรือไม่
                if (!snapshot.Exists)
                {
                    return NotFound(new { Success = false, Message = "User not found." });
                }

                // ลบเอกสารจาก Firestore
                await docRef.DeleteAsync();

                return Ok(new { Success = true, Message = "User deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred while deleting the user: {ex.Message}" });
            }
        }

        //ลบข้อมูลทั้งหมด และรีเซ็ท id !becarefull!
        [HttpDelete("reset-user")]
        public async Task<IActionResult> ResetUser([FromQuery] bool confirm = false)
        {
            if (!confirm)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Confirmation is required to reset user!"
                });
            }
            try
            {
                // ลบข้อมูลทั้งหมดใน Collection "users"
                var branchCollection = _firestoreDb.Collection("users");
                var snapshot = await branchCollection.GetSnapshotAsync();
                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.DeleteAsync();
                }

                // รีเซ็ตค่า lastUserId ใน Collection "settings"
                var settingsCollection = _firestoreDb.Collection("settings");
                var idDocRef = settingsCollection.Document("userIdTracking");

                await idDocRef.SetAsync(new { lastUserId = 0 }); // รีค่าเป็น 0

                return Ok(new
                {
                    Success = true,
                    Message = "All user deleted and ID reset successfully!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while resetting user: " + ex.Message
                });
            }
        }
    }
}