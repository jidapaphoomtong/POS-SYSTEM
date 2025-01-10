using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Filters;
using backend.Models;
using backend.Services;
using backend.Services.AdminService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [LogAction]
    // [ServiceFilter(typeof(CheckHeaderAttribute))]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // เพิ่ม Branch เข้า Collection "branches"
        [CustomAuthorizeRole("Admin")] // ระบุว่าต้องเป็น Role "Admin" เท่านั้นถึงเข้าถึงได้
        [HttpPost("add-branch")]
        public async Task<IActionResult> AddBranch([FromBody] Branch branch)
        {
            // ดึงข้อมูล User จาก Claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;                   // Role จาก Jwt Token
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;                   // ชื่อ user จาก Jwt Token

            // เรียกใช้ Service เพิ่มสาขา (Branch)
            var response = await _adminService.AddBranch(branch);

            if (response.Success)
                return Ok(new { Success = true, Message = response.Message, BranchId = response.Data });

            return BadRequest(new { Success = false, Message = response.Message });
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("add-employee/{branchId}")]
        public async Task<IActionResult> AddEmployee(string branchId, [FromBody] Employee employee)
        {
            try
            {
                // ตรวจสอบสิทธิ์ของผู้ใช้
                var userName = User.FindFirst(ClaimTypes.Name)?.Value; 
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
                {
                    return Forbid("You do not have permission to add employees.");
                }

                // สร้าง Salt และ Hash Password
                string salt = GenerateSalt();
                string hashedPassword = HashPassword(employee.passwordHash, salt);

                // เรียกใช้งาน Service Layer เพื่อเพิ่มพนักงาน
                var response = await _adminService.AddEmployee(branchId, employee);

                // ตรวจสอบผลลัพธ์
                if (response.Success) return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // รวม password กับ salt
                var saltedPassword = password + salt;
                byte[] saltedPasswordBytes = System.Text.Encoding.UTF8.GetBytes(saltedPassword);

                // แฮชรหัสผ่าน
                byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashBytes); // แปลงเป็น Base64 เพื่อการเก็บในฐานข้อมูล
            }
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes); // แปลงเป็น Base64 เพื่อให้อ่านง่ายและเก็บในฐานข้อมูล
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("add-product/{branchId}")]
        public async Task<IActionResult> AddProduct(string branchId, [FromBody] Products product)
        {
            // ดึง User จาก Claim
            var userName = User.FindFirst(ClaimTypes.Name)?.Value; // ชื่อ user จาก Jwt Token
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value; // ดึง Role จาก Claim
            
            var response = await _adminService.AddProduct(branchId, product);
            if (response.Success) return Ok(response);
            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin")]
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            // ตรวจสอบว่าผู้ใช้ Authenticated หรือไม่
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Success = false, Message = "Unauthorized. Please login." });
            }

            // ดึงข้อมูล User และ Role
            var user = User.Identity?.Name; // ดึงชื่อผู้ใช้
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value); // ดึง Role

            // Log ข้อมูล User และ Role
            Console.WriteLine($"User: {user}, Roles: {string.Join(", ", roles)}");

            // // ตรวจสอบ Role ว่ามีสิทธิ์เข้าถึงหรือไม่
            // if (!roles.Contains("Admin"))
            // {
            //     return Forbid(new { Success = false, Message = "Access Denied: Admin role required." });
            // }

            // เรียก Service เพื่อดึงข้อมูล Branches
            var response = await _adminService.GetBranches();

            // ตอบกลับผลลัพธ์
            if (response.Success)
            {
                Console.WriteLine("Branches fetched successfully.");
                return Ok(new { success = true, data = response.Data });
            }
            else
            {
                Console.WriteLine($"Error fetching branches: {response.Message}");
                return BadRequest(new { success = false, message = response.Message });
            }
        }

        // อัปเดต Branch
        [CustomAuthorizeRole("Admin")]
        [HttpPut("branches/{branchId}")]
        public async Task<IActionResult> UpdateBranch(string branchId, [FromBody] Branch updatedBranch)
        {
            try
            {
                // ตรวจสอบข้อมูล branch
                if (string.IsNullOrWhiteSpace(updatedBranch.Name) || 
                    string.IsNullOrWhiteSpace(updatedBranch.Location) || 
                    string.IsNullOrWhiteSpace(updatedBranch.IconUrl))
                {
                    return BadRequest(new { Success = false, Message = "Invalid branch data." });
                }

                // ดึงข้อมูล branch เดิมจาก Firestore
                var existingBranchResponse = await _adminService.GetBranchById(branchId);
                if (!existingBranchResponse.Success)
                {
                    return NotFound(new { Success = false, Message = "Branch not found." });
                }

                // สร้าง branch ที่ต้องการอัปเดต
                var branchToUpdate = new Branch
                {
                    Name = string.IsNullOrWhiteSpace(updatedBranch.Name) ? existingBranchResponse.Data.Name : updatedBranch.Name,
                    Location = string.IsNullOrWhiteSpace(updatedBranch.Location) ? existingBranchResponse.Data.Location : updatedBranch.Location,
                    IconUrl = string.IsNullOrWhiteSpace(updatedBranch.IconUrl) ? existingBranchResponse.Data.IconUrl : updatedBranch.IconUrl,
                };

                // อัปเดต branch ใน Firestore
                var updateResponse = await _adminService.UpdateBranch(branchId, branchToUpdate);
                if (!updateResponse.Success)
                {
                    return BadRequest(new { Success = false, Message = "Failed to update branch." });
                }

                return Ok(new { Success = true, Message = "Branch updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin")]
        [HttpGet("branches/{branchId}")]
        public async Task<IActionResult> GetBranchById(string branchId)
        {
            try
            {
                // Log ID ที่เรียก
                Console.WriteLine($"Requesting branch with ID: {branchId}");

                // เรียกใช้ AdminService เพื่อตรวจสอบ branch
                var response = await _adminService.GetBranchById(branchId);

                // ตรวจสอบว่ามีข้อมูลหรือไม่
                if (!response.Success)
                {
                    return NotFound(new { Success = false, Message = response.Message });
                }

                return Ok(new { Success = true, Data = response.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // ลบ Branch
        [CustomAuthorizeRole("Admin")]
        [HttpDelete("branches/{branchId}")]
        public async Task<IActionResult> DeleteBranch(string branchId)
        {
            try
            {
                await _adminService.DeleteBranch(branchId);
                return Ok(new { message = "Branch deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // พนักงาน
        [CustomAuthorizeRole("Admin, Manager")]
        [HttpGet("branches/{branchId}/employees")]
        public async Task<IActionResult> GetEmployees(string branchId)
        {
            var response = await _adminService.GetEmployees(branchId);
            if (!response.Success)
            {
                return NotFound(new { Success = false, Message = response.Message });
            }
            
            return Ok(new
            {
                Success = true,
                Message = response.Message,
                Data = response.Data // จะส่งกลับเป็นรูปแบบรายการ
            });
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpGet("getbyemail")]
        public async Task<IActionResult> GetEmployeeByEmail([FromQuery] string branchId, string email)
        {
            var response = await _adminService.GetEmployeeByEmail(branchId ,email);

            if (response.Success)
            {
                return Ok(response.Data); // คืนค่าพนักงานในรูปแบบ JSON
            }

            return NotFound(response.Message); // ส่งคืนข้อความถ้าไม่พบพนักงาน
        }


        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPut("branches/{branchId}/employees/{employeeId}")]
        public async Task<IActionResult> UpdateEmployee(string branchId, string employeeId, [FromBody] Employee updatedEmployee)
        {
            try
            {
                await _adminService.UpdateEmployee(branchId, employeeId, updatedEmployee);
                return Ok(new { message = "Employee updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("branches/{branchId}/employees/{employeeId}")]
        public async Task<IActionResult> DeleteEmployee(string branchId, string employeeId)
        {
            try
            {
                await _adminService.DeleteEmployee(branchId, employeeId);
                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // สินค้า
        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("branches/{branchId}/products")]
        public async Task<IActionResult> GetProducts(string branchId)
        {
            try
            {
                var result = await _adminService.GetProducts(branchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPut("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> UpdateProduct(string branchId, string productId, [FromBody] Products updatedProduct)
        {
            try
            {
                await _adminService.UpdateProduct(branchId, productId, updatedProduct);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("branches/{branchId}/products/{productId}")]
        public async Task<IActionResult> DeleteProduct(string branchId, string productId)
        {
            try
            {
                await _adminService.DeleteProduct(branchId, productId);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //**
            //Employee
        //**

        // [CustomAuthorizeRole("Admin")]
        // [HttpDelete("deleteall/{branchId}")]
        // public async Task<IActionResult> DeleteAllEmployees(string branchId)
        // {
        //     // ตรวจสอบว่า branchId ไม่เป็น null หรือว่างเปล่า
        //     if (string.IsNullOrWhiteSpace(branchId))
        //     {
        //         return BadRequest("Branch ID cannot be null or empty.");
        //     }

        //     try
        //     {
        //         var response = await _adminService.DeleteAllEmployees(branchId);

        //         if (response.Success)
        //         {
        //             return Ok(response.Message); // ส่งค่าความสำเร็จกลับ
        //         }

        //         return BadRequest(response.Message); // ส่งค่าข้อผิดพลาดกลับ
        //     }
        //     catch (Exception ex)
        //     {
        //         // บันทึกข้อผิดพลาด
        //         // Logger.LogError(ex, "Error deleting employees for branch {branchId}", branchId); // ใช้ logging ตามที่ได้ตั้งไว้
        //         return StatusCode(500, "An unexpected error occurred. Please try again later."); // ข้อผิดพลาดที่ไม่คาดคิด
        //     }
        // }

        // [CustomAuthorizeRole("Admin")]
        // [HttpPost("reset-employee-sequence")]
        // public async Task<IActionResult> ResetEmployeeId(string branchId)
        // {
        //     var response = await _adminService.ResetEmployeeId(branchId);

        //     if (response.Success)
        //     {
        //         return Ok(new { Success = true, Message = response.Message });
        //     }

        //     return BadRequest(new { Success = false, Message = response.Message });
        // }

        //**
            //Branch
        //**

        // [CustomAuthorizeRole("Admin")]
        // [HttpDelete("branches")]
        // public async Task<IActionResult> DeleteAllBranches()
        // {
        //     var response = await _adminService.DeleteAllBranches();

        //     if (response.Success)
        //     {
        //         return Ok(new { Success = true, Message = response.Message });
        //     }

        //     return BadRequest(new { Success = false, Message = response.Message });
        // }

        // [CustomAuthorizeRole("Admin")]
        // [HttpPost("reset-branch-sequence")]
        // public async Task<IActionResult> ResetBranchIdSequence()
        // {
        //     var response = await _adminService.ResetBranchIdSequence();

        //     if (response.Success)
        //     {
        //         return Ok(new { Success = true, Message = response.Message });
        //     }

        //     return BadRequest(new { Success = false, Message = response.Message });
        // }
    }
}