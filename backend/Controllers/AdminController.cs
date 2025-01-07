using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using backend.Services.AdminService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [LogAction]
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
            // ดึง User จาก Claim
            var userName = User.FindFirst(ClaimTypes.Name)?.Value; // ชื่อ user จาก Jwt Token
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value; // ดึง Role จาก Claim

            // เรียกใช้งาน Service Layer เพื่อทำธุรกิจ
            var response = await _adminService.AddEmployee(branchId, employee);

            // ตรวจสอบผลลัพธ์
            if (response.Success) return Ok(response); // สำเร็จ
            return BadRequest(response);              // ล้มเหลว
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
        public async Task<IActionResult> UpdateBranch(string branchId, [FromBody] Dictionary<string, object> updatedData)
        {
            try
            {
                await _adminService.UpdateBranch(branchId, updatedData);
                return Ok(new { message = "Branch updated successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
            try
            {
                var result = await _adminService.GetEmployees(branchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        // [Authorize(Roles = "admin")]
        // [HttpGet("view-bran")]
        // public IActionResult ViewBranch()
        // {
        //     return Ok(new List<string> { "Branch A", "Branch B", "Branch C" });
        // }

        // [Authorize(Roles = "manager")]
        // [HttpGet("view-employee")]
        // public IActionResult ViewEmployee()
        // {
        //     return Ok(new List<string> { "Employee A", "Employee B", "Employee C" });
        // }

        // [Authorize(Roles = "employee, manager")]
        // [HttpGet("view-products")]
        // public IActionResult ViewProducts()
        // {
        //     return Ok(new List<string> { "Product A", "Product B", "Product C" });
        // }
    }
}