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
using backend.Filters;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using backend.Services.EmployeeService;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [DisableCors]
    [LogAction]
    // [ServiceFilter(typeof(CheckHeaderAttribute))]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IEmployeeService _employeeService;

        public AuthController(IAuthService authService, ITokenService tokenService, IEmployeeService employeeService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _employeeService = employeeService;
        }

        // Login (JWT-based)
        // [AllowAnonymous]
        // [HttpPost("login")]
        // public async Task<IActionResult> Login([FromBody] Login userLogin)
        // {
        //     // Validate ModelState
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(new { Success = false, Message = "Invalid input data." });
        //     }

        //     // ตรวจสอบผู้ใช้
        //     var userSnapshot = await _authService.GetUserByEmail(userLogin.Email);
        //     if (userSnapshot == null)
        //     {
        //         return Unauthorized(new { Success = false, Message = "User not found." });
        //     }

        //     var user = userSnapshot.ToDictionary(); // Convert snapshot to Dictionary
        //     if (!_authService.VerifyPassword(userLogin.Password, user["passwordHash"].ToString(), user["salt"].ToString()))
        //     {
        //         return Unauthorized(new { Success = false, Message = "Invalid credentials." });
        //     }

        //     // ตรวจสอบ Role ในข้อมูลผู้ใช้
        //     string role = user.ContainsKey("roles") && user["roles"] is IList<object> roles && roles.Count > 0
        //     ? (roles.First() as Dictionary<string, object>)?["Name"]?.ToString() ?? RoleName.Employee
        //     : RoleName.Employee;

        //     // ตั้ง Claim JWT
        //     var claims = new List<Claim>
        //     {
        //         new Claim(ClaimTypes.Name, user["firstName"].ToString()),
        //         new Claim(ClaimTypes.Email, userLogin.Email),
        //         new Claim(ClaimTypes.NameIdentifier, userSnapshot.Id),
        //         new Claim(ClaimTypes.Role, role), // เพิ่ม Role
        //         // new Claim("branchId", user.ContainsKey("branchId") ? user["branchId"].ToString() : "") // เพิ่ม Branch ID
        //     };

        //     // Generate Token
        //     var accessToken = _tokenService.GenerateAccessToken(claims);

        //     // ตั้งค่าสำหรับ Cookies
        //     var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //     var principal = new ClaimsPrincipal(identity);
        //     var authProperties = new AuthenticationProperties
        //     {
        //         IsPersistent = true, // ให้ Cookies มีอายุใช้งาน
        //         ExpiresUtc = DateTime.UtcNow.AddDays(7) // อายุ Cookies
        //     };

        //     await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        //     return Ok(new {
        //         Success = true,
        //         Message = "Login successful.",
        //         Token = accessToken, // Token สำหรับใช้ใน Client-Side Authentication
        //         Expiration = authProperties.ExpiresUtc,
        //         Role = role, // ระยะเวลาหมดอายุของเซสชัน
        //         // Cookies = responseCookies // คืนค่าข้อมูล Cookies กลับให้ผู้ใช้ดูใน Swagger
        //     });
        // }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login userLogin)
        {
            // Validate ModelState
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(userLogin.Email) || string.IsNullOrWhiteSpace(userLogin.Password))
            {
                return BadRequest(new { Success = false, Message = "Please provide both email and password." });
            }

            // ตรวจสอบผู้ใช้ใน collection users
            var userSnapshot = await _authService.GetUserByEmail(userLogin.Email);
            string role;
            string branchId = userLogin.branchId;

            if (userSnapshot != null)
            {
                var user = userSnapshot.ToDictionary();
                Console.WriteLine(JsonConvert.SerializeObject(user));

                if (!_authService.VerifyPassword(userLogin.Password, user["passwordHash"].ToString(), user["salt"].ToString()))
                {
                    return Unauthorized(new { Success = false, Message = "Invalid credentials." });
                }

                role = user.ContainsKey("roles") && user["roles"] is IList<object> roles && roles.Count > 0
                    ? (roles.First() as Dictionary<string, object>)?["Name"]?.ToString() ?? RoleName.Employee
                    : RoleName.Employee;

                // ตรวจสอบ branchId (กรณีที่ต้องการ)
                if (user.ContainsKey("branchId"))
                {
                    branchId = user["branchId"].ToString();
                }
                
            }
            else
            {
                // ตรวจสอบใน sub-collection employees
                if (string.IsNullOrWhiteSpace(branchId))
                {
                    return BadRequest(new { Success = false, Message = "Branch ID is required for employee lookup." });
                }

                var employeeResponse = await _employeeService.GetEmployeeByEmail(branchId, userLogin.Email);
                // Console.WriteLine(JsonConvert.SerializeObject(employeeResponse));
                if (!employeeResponse.Success)
                {
                    return Unauthorized(new { Success = false, Message = "User not found in employees." });
                }

                var employee = employeeResponse.Data;

                // ตรวจสอบรหัสผ่าน
                if (!_employeeService.VerifyPassword(userLogin.Password, employee.passwordHash, employee.salt))
                {
                    return Unauthorized(new { Success = false, Message = "Invalid credentials." });
                }

                // // ตรวจสอบบทบาทของพนักงาน
                if (employee.roles != null && employee.roles.Any())
                {
                    role = employee.roles.First().Name; // ดึงชื่อบทบาทจาก Role แรก
                }
                else
                {
                    role = RoleName.Employee; // ถ้าไม่มี ให้ใช้ค่าเริ่มต้น
                }
                
                // role = RoleName.Employee; 
                branchId = employee.branchId;
            }

            // สร้าง Claim JWT
            var claims = new List<Claim>
            {
                // new Claim(ClaimTypes.Name, userLogin.Email ?? throw new ArgumentNullException("Email cannot be null.")),
                new Claim(ClaimTypes.Email, userLogin.Email ?? throw new ArgumentNullException("Email cannot be null.")),
                new Claim(ClaimTypes.Role, role ?? throw new ArgumentNullException("Role cannot be null.")), // ตรวจสอบ role
            };

            if (!string.IsNullOrWhiteSpace(branchId))
            {
                claims.Add(new Claim("branchId", branchId));
            }

            // Generate Token
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

            return Ok(new
            {
                Success = true,
                Message = "Login successful.",
                Token = accessToken,
                Role = role,
                BranchId = branchId
            });
        }

        // private string GenerateUserToken(string email, string role, string branchId)
        // {
        //     var claims = new List<Claim>
        //     {
        //         new Claim(ClaimTypes.Name, email),
        //         new Claim(ClaimTypes.Email, email),
        //         new Claim(ClaimTypes.Role, role),
        //         new Claim("branchId", branchId)
        //     };

        //     return _tokenService.GenerateAccessToken(claims);
        // }

        // private string GenerateEmployeeToken(string email, string role, string branchId)
        // {
        //     var claims = new List<Claim>
        //     {
        //         new Claim(ClaimTypes.Name, email),
        //         new Claim(ClaimTypes.Email, email),
        //         new Claim(ClaimTypes.Role, role),
        //         new Claim("branchId", branchId)
        //     };

        //     return _tokenService.GenerateAccessToken(claims);
        // }


        [AllowAnonymous]
        [HttpPost("register")]
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
            return Ok(new { Message = "Logout successfully!" });
        }

        // Get User Data from JWT Claims
        [HttpGet("get-user-data")]
        [Authorize]
        public async Task<IActionResult> GetUserData()
        {
            try
            {
                // ดึง User ID จาก JWT Token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Token does not contain user ID" });

                // ค้นหาเอกสารผู้ใช้ใน Firestore
                var userSnapshot = await _authService.GetUserById(userId);
                if (userSnapshot == null)
                    return NotFound(new { Success = false, Message = "User not found" });

                var user = userSnapshot.ToDictionary();

                // รองรับกรณี roles เป็น List<object>
                var roles = user.ContainsKey("roles")
                    ? ((List<object>)user["roles"]).OfType<Dictionary<string, object>>().ToList()
                    : new List<Dictionary<string, object>>();

                var mainRole = roles.FirstOrDefault()?["Name"]?.ToString() ?? "employee";

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        Name = user["firstName"],
                        Email = user["email"],
                        Role = mainRole
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred.", Details = ex.Message });
            }
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
        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            try
            {
                // ตรวจสอบว่า Role ที่ส่งมาว่าไม่เป็นค่าว่าง
                if (updatedUser.roles.Any(r => string.IsNullOrWhiteSpace(r.Name)))
                {
                    return BadRequest(new { Success = false, Message = "Invalid Role detected." });
                }

                // ดึงข้อมูลผู้ใช้เดิมจากฐานข้อมูล
                var existingUserSnapshot = await _authService.GetUserById(id);
                if (existingUserSnapshot == null)
                {
                    return NotFound(new { Success = false, Message = "User not found." });
                }
                var existingUser = existingUserSnapshot.ToDictionary();

                // อัปเดตเฉพาะฟิลด์ที่มีการเปลี่ยนแปลง
                var userToUpdate = new User
                {
                    firstName = string.IsNullOrWhiteSpace(updatedUser.firstName)
                                ? existingUser["firstName"].ToString()
                                : updatedUser.firstName,
                    lastName = string.IsNullOrWhiteSpace(updatedUser.lastName)
                                ? existingUser["lastName"].ToString()
                                : updatedUser.lastName,
                    email = string.IsNullOrWhiteSpace(updatedUser.email)
                            ? existingUser["email"].ToString()
                            : updatedUser.email,
                    roles = updatedUser.roles.Any()
                            ? updatedUser.roles
                            : (List<Role>)existingUser["roles"]
                };

                // ส่งข้อมูลไปยัง AuthService เพื่ออัปเดตข้อมูล
                var success = await _authService.UpdateUserAsync(id, userToUpdate);
                if (!success)
                {
                    return NotFound(new { Success = false, Message = "User not found or cannot update." });
                }

                return Ok(new { Success = true, Message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // เฉพาะ Admin เท่านั้นที่ลบผู้ใช้ได้
        [Authorize]
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
        [Authorize]
        [HttpDelete("reset-all")]
        public async Task<IActionResult> ResetAll()
        {
            try
            {
                // ลบผู้ใช้ทั้งหมดใน Firestore
                var deleted = await _authService.DeleteAllUsersAsync();

                // ตั้งค่า ID ให้เริ่มต้นใหม่
                await _authService.ResetUserIdSequence();

                return Ok(new { Success = true, Message = "All users have been deleted and ID sequence refreshed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Unable to reset users: " + ex.Message });
            }
        }
    }
}
