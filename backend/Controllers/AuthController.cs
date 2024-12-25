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
                var settingsCollection = _firestoreDb.Collection("settings"); // คอลเลกชันสำหรับเก็บค่า Id ล่าสุด
                var idDocRef = settingsCollection.Document("userIdTracking");
                var idSnapshot = await idDocRef.GetSnapshotAsync();

                int newId;
                if (idSnapshot.Exists)
                {
                    // ดึงค่า Id ล่าสุด
                    var currentId = idSnapshot.GetValue<int>("lastUserId");
                    newId = currentId + 1;
                }
                else
                {
                    // กำหนดค่าเริ่มต้นของ Id
                    newId = 1;
                }

                // อัปเดตค่า Id ล่าสุดกลับไปที่ Firestore
                await idDocRef.SetAsync(new { lastUserId = newId });
                // ---- จบส่วนการจัดการ Id อัตโนมัติ ----

                // เพิ่มผู้ใช้ใหม่
                var newUser = await userCollection.AddAsync(new
                {
                    id = newId, // ใช้ค่า Id ที่รันอัตโนมัติ
                    fullName = userRegister.FullName,
                    email = userRegister.Email,
                    password = userRegister.Password, // ในโปรเจกต์จริงควรเข้ารหัสรหัสผ่าน (เช่น Hash)
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

        // [HttpPost("add-user")]
        // public async 

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
                // อ้างถึง document ที่ต้องการอัปเดตด้วย documentId
                var docRef = _firestoreDb.Collection("users").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                // ตรวจสอบว่าผู้ใช้ที่ต้องการแก้ไขมีอยู่หรือไม่
                if (!snapshot.Exists)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // สร้าง Dictionary สำหรับเก็บข้อมูลที่จะอัปเดต
                var updateData = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(updateUser.FullName))
                    updateData["fullName"] = updateUser.FullName;

                if (!string.IsNullOrWhiteSpace(updateUser.Email))
                    updateData["email"] = updateUser.Email;

                if (!string.IsNullOrWhiteSpace(updateUser.Password))
                    updateData["password"] = updateUser.Password;

                // ตรวจสอบว่ามีข้อมูลที่จะอัปเดตหรือไม่
                if (updateData.Count == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "No valid fields specified for update."
                    });
                }

                // อัปเดตข้อมูลใน Firestore
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
    }
}

//ตอนนี้ไม่ปลอดภัย!!!!!