using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using backend.Services.AuthService;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register userRegister)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                if (await _authService.IsEmailRegistered(userRegister.email))
                {
                    return Conflict(new { Success = false, Message = "Email already registered." });
                }

                var newId = await _authService.GetNextUserId();
                var salt = _authService.GenerateSalt();
                var hashedPassword = _authService.HashPassword(userRegister.password, salt);

                // var newUserRef = await _authService.RegisterUserAsync(newId, userRegister.FullName, userRegister.Email, salt, hashedPassword);
                var newUserRef = await _authService.RegisterUserAsync(newId, userRegister.firstName, userRegister.lastName ,userRegister.email, salt, hashedPassword);

                return Ok(new { Success = true, Message = "User registered successfully!", UserId = newId, DocumentId = newUserRef.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred during registration. " + ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userSnapshot = await _authService.GetUserByEmail(userLogin.Email);

            if (userSnapshot == null)
            {
                return NotFound(new { Success = false, Message = "User not found" });
            }

            var user = userSnapshot.ToDictionary();
            var salt = (string)user["salt"];
            var passwordHash = (string)user["passwordHash"];

            if (!_authService.VerifyPassword(userLogin.Password, passwordHash, salt))
            {
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("5MZHydfAoWBsruaAXLex4omTno0zhkX9zMGRXUTZ");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userSnapshot.Id),
                    new Claim(ClaimTypes.Email, (string)user["email"])
                }),
                Expires = DateTime.UtcNow.AddDays(30),
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
    }

        // [HttpGet("get-user")]
        // public async Task<IActionResult> GetUser()
        // {
        //     try
        //     {
        //         // สร้างอ้างอิงถึง collection "users"
        //         var userCollection = _firestoreDb.Collection("users");

        //         // ดึงข้อมูลจาก collection
        //         var snapshot = await userCollection.GetSnapshotAsync();

        //         // ตรวจสอบว่ามีข้อมูลหรือไม่
        //         if (!snapshot.Documents.Any())
        //         {
        //             return NotFound(new { Success = false, Message = "No users found in Firestore." });
        //         }

        //         // แปลง snapshot เป็นข้อมูลที่ต้องการส่งออก (List ของ users)
        //         var users = snapshot.Documents
        //             .Select(doc => new 
        //             { 
        //                 Id = doc.Id, 
        //                 Fields = doc.ToDictionary() 
        //             })
        //             .ToList();

        //         // ส่งข้อมูลสำเร็จพร้อมรายละเอียด
        //         return Ok(new { Success = true, Documents = users });
        //     }
        //     catch (Exception ex)
        //     {
        //         // ในกรณีที่มีปัญหาอื่นๆ
        //         return BadRequest(new { Success = false, Message = ex.Message });
        //     }
        // }

        // [HttpPut("update/{documentId}")]
        // public async Task<IActionResult> UpdateUser(string documentId, [FromBody] UpdateUser updateUser)
        // {
        //     try
        //     {
        //         var docRef = _firestoreDb.Collection("users").Document(documentId);
        //         var snapshot = await docRef.GetSnapshotAsync();

        //         if (!snapshot.Exists)
        //         {
        //             return NotFound(new
        //             {
        //                 Success = false,
        //                 Message = "User not found."
        //             });
        //         }

        //         var updateData = new Dictionary<string, object>();

        //         if (!string.IsNullOrWhiteSpace(updateUser.FullName))
        //             updateData["fullName"] = updateUser.FullName;

        //         if (!string.IsNullOrWhiteSpace(updateUser.Email))
        //             updateData["email"] = updateUser.Email;

        //         if (!string.IsNullOrWhiteSpace(updateUser.Password))
        //         {
        //             var newSalt = GenerateSalt();
        //             var newHashedPassword = HashPassword(updateUser.Password, newSalt);
        //             updateData["salt"] = newSalt;
        //             updateData["passwordHash"] = newHashedPassword;
        //         }

        //         if (updateData.Count == 0)
        //         {
        //             return BadRequest(new
        //             {
        //                 Success = false,
        //                 Message = "No valid fields specified for update."
        //             });
        //         }

        //         await docRef.UpdateAsync(updateData);

        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "User updated successfully!"
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = $"An error occurred while updating the user: {ex.Message}"
        //         });
        //     }
        // }

        // [HttpDelete("delete/{documentId}")]
        // public async Task<IActionResult> DeleteUser(string documentId)
        // {
        //     try
        //     {
        //         if (string.IsNullOrWhiteSpace(documentId))
        //         {
        //             return BadRequest(new { Success = false, Message = "Document ID is required for deleting a user." });
        //         }

        //         // อ้างถึงเอกสารใน Firestore ด้วย documentId
        //         var docRef = _firestoreDb.Collection("users").Document(documentId);
        //         var snapshot = await docRef.GetSnapshotAsync();

        //         // ตรวจสอบว่ามีเอกสารที่ตรงกับ documentId หรือไม่
        //         if (!snapshot.Exists)
        //         {
        //             return NotFound(new { Success = false, Message = "User not found." });
        //         }

        //         // ลบเอกสารจาก Firestore
        //         await docRef.DeleteAsync();

        //         return Ok(new { Success = true, Message = "User deleted successfully!" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { Success = false, Message = $"An error occurred while deleting the user: {ex.Message}" });
        //     }
        // }

        // //ลบข้อมูลทั้งหมด และรีเซ็ท id !becarefull!
        // [HttpDelete("reset-user")]
        // public async Task<IActionResult> ResetUser([FromQuery] bool confirm = false)
        // {
        //     if (!confirm)
        //     {
        //         return BadRequest(new
        //         {
        //             Success = false,
        //             Message = "Confirmation is required to reset user!"
        //         });
        //     }
        //     try
        //     {
        //         // ลบข้อมูลทั้งหมดใน Collection "users"
        //         var branchCollection = _firestoreDb.Collection("users");
        //         var snapshot = await branchCollection.GetSnapshotAsync();
        //         foreach (var doc in snapshot.Documents)
        //         {
        //             await doc.Reference.DeleteAsync();
        //         }

        //         // รีเซ็ตค่า lastUserId ใน Collection "settings"
        //         var settingsCollection = _firestoreDb.Collection("settings");
        //         var idDocRef = settingsCollection.Document("userIdTracking");

        //         await idDocRef.SetAsync(new { lastUserId = 0 }); // รีค่าเป็น 0

        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "All user deleted and ID reset successfully!"
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while resetting user: " + ex.Message
        //         });
        //     }
        // }
}
