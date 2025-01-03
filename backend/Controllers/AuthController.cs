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
using backend.Services.Tokenservice;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Google.Apis.Auth.OAuth2.Requests;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        // Login (JWT-based)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login userLogin)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Invalid input data." });

            // ตรวจสอบข้อมูลผู้ใช้
            var userSnapshot = await _authService.GetUserByEmail(userLogin.Email);
            if (userSnapshot == null)
                return Unauthorized(new { Success = false, Message = "User not found." });

            var user = userSnapshot.ToDictionary();
            if (!_authService.VerifyPassword(userLogin.Password, user["passwordHash"].ToString(), user["salt"].ToString()))
                return Unauthorized(new { Success = false, Message = "Invalid credentials." });

            // ตรวจสอบ Role ของผู้ใช้
            string role = user.ContainsKey("role") ? user["role"].ToString() : RoleName.Employee;

            // สร้าง Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user["firstName"].ToString()),
                new Claim(ClaimTypes.Email, userLogin.Email),
                new Claim(ClaimTypes.NameIdentifier, userSnapshot.Id),
                new Claim(ClaimTypes.Role, role) // เพิ่ม Role
            };

            // สร้าง Access Token
            var accessToken = _tokenService.GenerateAccessToken(claims);

            // ตั้งค่าสำหรับ Cookies
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // ให้ Cookies มีอายุใช้งาน
                ExpiresUtc = DateTime.UtcNow.AddDays(7) // อายุ Cookies
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return Ok(new { Token = accessToken });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] User userRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // ตรวจสอบว่ามี Email ซ้ำหรือไม่
            if (await _authService.IsEmailRegistered(userRegister.email))
                return Conflict(new { Success = false, Message = "Email already exists." });

            try
            {
                // สร้างข้อมูลผู้ใช้ใหม่
                var newId = await _authService.GetNextUserId();
                var salt = _authService.GenerateSalt();
                var hashedPassword = _authService.HashPassword(userRegister.password, salt);

                var defaultRole = new List<Role>(); // เพิ่ม Role ให้ Default เป็น Employee

                var userRef = await _authService.RegisterUserAsync(
                    newId,
                    userRegister.firstName,
                    userRegister.lastName,
                    userRegister.email,
                    salt,
                    hashedPassword,
                    defaultRole // เพิ่ม Role
                );

                return Ok(new { Success = true, Message = "User registered successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Registration failed.", Details = ex.Message });
            }
        }

        // Logout (Cookies-based)
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logged out successfully!" });
        }

        // Get User Data from JWT Claims
        [Authorize]
        [HttpGet("get-user-data")]
        public IActionResult GetUserData()
        {
            var claims = HttpContext.User.Identity as ClaimsIdentity;
            if (claims == null)
            return Unauthorized(new { Success = false, Message = "Unauthorized" });

            var email = claims.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var name = claims.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var role = claims.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            return Ok(new { Success = true, Data = new { Name = name, Email = email, Role = role } });
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

        // Admin และ Manager สามารถแก้ไขข้อมูลผู้ใช้
        [HttpPut("update/{id}")]
        // [Authorize(Roles = "admin")] // Admin เท่านั้นที่สามารถอัปเดตข้อมูลได้
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            try
            {
                // ตรวจสอบ Role ที่ส่งมาว่าไม่เป็นค่าว่าง
                if (updatedUser.roles.Any(r => string.IsNullOrWhiteSpace(r.Name)))
                {
                    return BadRequest(new { Success = false, Message = "Invalid Role detected." });
                }

                // ตรวจสอบ Email ว่าซ้ำหรือไม่
                var userWithSameEmail = await _authService.IsEmailRegistered(updatedUser.email);
                if (userWithSameEmail && id != updatedUser.email)
                {
                    return Conflict(new { Success = false, Message = "Email already exists." });
                }

                // เรียก AuthService เพื่ออัปเดตข้อมูล
                var success = await _authService.UpdateUserAsync(id, updatedUser);
                if (!success)
                {
                    return NotFound(new { Success = false, Message = "User not found or cannot update." });
                }

                return Ok(new { Success = true, Message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while updating user: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // เฉพาะ Admin เท่านั้นที่ลบผู้ใช้ได้
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                // ลบข้อมูลผู้ใช้ใน Firestore
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

        // รีเซ็ตผู้ใช้ทั้งหมด (เฉพาะ Admin)
        [HttpDelete("reset-all")]
        public async Task<IActionResult> ResetAll()
        {
            try
            {
                // ลบผู้ใช้ทั้งหมดใน Firestore
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
