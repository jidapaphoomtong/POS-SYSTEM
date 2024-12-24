using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

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

                // เพิ่มผู้ใช้ใหม่
                var newUser = await userCollection.AddAsync(new
                {
                    fullName = userRegister.FullName,
                    userRole = userRegister.UserRole,
                    email = userRegister.Email,
                    password = userRegister.Password, // ในโปรเจกต์จริงควรเข้ารหัสรหัสผ่าน (เช่น Hash)
                });

                return Ok(new { Success = true, Message = "User registered successfully!", UserId = newUser.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred during registration. " + ex.Message });
            }
        }

        [HttpPost("login")]
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
                return NotFound("User not found");
            }

            var user = snapshot.Documents.First().ConvertTo<Dictionary<string, object>>();
            if ((string)user["password"] != userLogin.Password)
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok("Login successful!");
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

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser updateUser)
        {
            try
            {
                // ตรวจสอบว่ามีอีเมลที่ต้องการอัปเดตอยู่ในระบบหรือไม่
                var userCollection = _firestoreDb.Collection("users");
                var snapshot = await userCollection.WhereEqualTo("email", updateUser.Email).GetSnapshotAsync();

                if (!snapshot.Documents.Any())
                {
                    return NotFound(new { Success = false, Message = "User not found." });
                }

                // อ้างถึงผู้ใช้งานที่ต้องการอัปเดต
                var doc = snapshot.Documents.First();

                // เตรียมตัวข้อมูลที่อัปเดต
                var updates = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(updateUser.FullName))
                {
                    updates["fullName"] = updateUser.FullName;
                }

                if (!string.IsNullOrWhiteSpace(updateUser.UserRole))
                {
                    updates["userRole"] = updateUser.UserRole;
                }

                if (!string.IsNullOrWhiteSpace(updateUser.Email))
                {
                    updates["email"] = updateUser.Email;
                }

                if (!string.IsNullOrWhiteSpace(updateUser.Password))
                {
                    updates["password"] = updateUser.Password;
                }

                if (updates.Count == 0)
                {
                    return BadRequest(new { Success = false, Message = "No valid fields specified for update." });
                }

                // อัปเดตผู้ใช้ใน Firestore
                await doc.Reference.UpdateAsync(updates);

                return Ok(new { Success = true, Message = "User updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred while updating the user: {ex.Message}" });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { Success = false, Message = "Email is required for deleting a user." });
            }

            try
            {
                // ตรวจสอบว่ามีผู้ใช้หรือไม่
                var userCollection = _firestoreDb.Collection("users");
                var snapshot = await userCollection.WhereEqualTo("email", email).GetSnapshotAsync();

                if (!snapshot.Documents.Any())
                {
                    return NotFound(new { Success = false, Message = "User not found." });
                }

                // ลบผู้ใช้
                var doc = snapshot.Documents.First();
                await doc.Reference.DeleteAsync();

                return Ok(new { Success = true, Message = "User deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred while deleting the user: {ex.Message}" });
            }
        }
    }
}