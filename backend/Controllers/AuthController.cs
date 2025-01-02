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
using Google.Cloud.Firestore;

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

        // // Cookies
        // [AllowAnonymous]
        // [HttpGet("~/login")]
        // public IActionResult Index() => View("_Login");

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Invalid input data." });
            }

            // ดึงข้อมูลผู้ใช้จาก Firestore โดยใช้ email
            var userSnapshot = await _authService.GetUserByEmail(userLogin.Email);
            if (userSnapshot == null)
            {
                return NotFound(new { Success = false, Message = "User not found." });
            }

            // แปลงข้อมูลจาก Firestore เป็น Dictionary
            var user = userSnapshot.ToDictionary();

            // ตรวจสอบว่า fields ที่จำเป็นทั้งหมดมีอยู่ในเอกสาร
            if (!user.ContainsKey("salt") || !user.ContainsKey("passwordHash"))
            {
                return StatusCode(500, new { Success = false, Message = "User data is incomplete or corrupted." });
            }

            // ตรวจสอบ password
            var salt = (string)user["salt"];
            var passwordHash = (string)user["passwordHash"];
            if (!_authService.VerifyPassword(userLogin.Password, passwordHash, salt))
            {
                return Unauthorized(new { Success = false, Message = "Invalid credentials." });
            }

            // ดึงข้อมูลเพิ่มเติม (firstname, lastname, role)
            // var firstName = user.ContainsKey("firstname") ? user["firstname"].ToString() : string.Empty;
            // var lastName = user.ContainsKey("lastname") ? user["lastname"].ToString() : string.Empty;
            // var role = user.ContainsKey("role") ? user["role"].ToString() : "User";

            // สร้าง JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("5MZHydfAoWBsruaAXLex4omTno0zhkX9zMGRXUTZ");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userSnapshot.Id), // ใส่ UserId
                    new Claim(ClaimTypes.Email, userLogin.Email),          // ใส่ Email
                    // new Claim(ClaimTypes.GivenName, firstName),            // ใส่ Firstname
                    // new Claim("lastname", lastName),                      // ใส่ Lastname เป็น Custom Claim
                    // new Claim(ClaimTypes.Role, role)                      // ใส่ Role
                }),
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = "localhost",
                Audience = "localhost",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // ส่ง JWT Token กลับไปยัง Client
            return Ok(new
            {
                Success = true,
                Message = "Login successful!",
                Token = tokenString
            });
        }

        [HttpPost("register")]
        // [Authorize(Policy = "AdminPolicy")]
        [Authorize]
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

                // CollectionReference user = _firestoreDb.Collection("users");
                // DocumentReference newUserRef = await user.AddAsync(userRegister);
                var newUserRef = await _authService.RegisterUserAsync(newId, userRegister.firstName, userRegister.lastName ,userRegister.email, salt, hashedPassword);

                return Ok(new { Success = true, Message = "User registered successfully!", UserId = newId, DocumentId = newUserRef.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred during registration. " + ex.Message });
            }
        }

        [HttpGet("get-user-data")]
        [Authorize] // Require JWT Token
        public IActionResult GetUserData()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var claims = identity.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var firstname = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
                var lastname = claims.FirstOrDefault(c => c.Type == "lastname")?.Value;
                var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        UserId = userId,
                        Email = email,
                        FirstName = firstname,
                        LastName = lastname,
                        Role = role
                    }
                });
            }

            return Unauthorized(new { Success = false, Message = "Unauthorized" });
        }

        [HttpGet("users")]
        [Authorize] // Optional: Only authorized users can view this
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                // ดึงข้อมูลจาก Firestore
                var users = await _authService.GetAllUsers();
                return Ok(new { Success = true, Users = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Unable to retrieve users: " + ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUser updateUser)
        {
            try
            {
                var updated = await _authService.UpdateUserAsync(id, updateUser);
                if (!updated)
                {
                    return NotFound(new { Success = false, Message = "User not found." });
                }

                return Ok(new { Success = true, Message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Unable to update user: " + ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var deleted = await _authService.DeleteUserAsync(id);
                if (!deleted)
                {
                    return NotFound(new { Success = false, Message = "User not found." });
                }

                return Ok(new { Success = true, Message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Unable to delete user: " + ex.Message });
            }
        }

        [HttpDelete("reset-all")]
        [Authorize]
        public async Task<IActionResult> ResetAll()
        {
            try
            {
                var deleted = await _authService.DeleteAllUsersAsync();
                return Ok(new { Success = true, Message = "All users have been deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Unable to reset users: " + ex.Message });
            }
        }
    }
}
