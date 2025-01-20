using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.EmployeeService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [DisableCors]
    [LogAction]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPost("add-employee/{branchId}")]
        public async Task<IActionResult> AddEmployee(string branchId, [FromBody] Employee employee)
        {
            try
            {
                // // ตรวจสอบสิทธิ์ของผู้ใช้
                // var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                // if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
                // {
                //     return Forbid("You do not have permission to add employees.");
                // }

                // // ตรวจสอบข้อมูลที่จำเป็น
                // if (string.IsNullOrWhiteSpace(branchId) || employee == null || 
                //     string.IsNullOrWhiteSpace(employee.email) || 
                //     string.IsNullOrWhiteSpace(employee.passwordHash))
                // {
                //     return BadRequest("BranchId, email, and password cannot be null or empty.");
                // }

                // ดึงข้อมูล User จาก Claims
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                // เรียกใช้งาน Service Layer เพื่อเพิ่มพนักงาน
                var response = await _employeeService.AddEmployee(branchId, employee);

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
        [HttpGet("branches/{branchId}/employees")]
        public async Task<IActionResult> GetEmployees(string branchId)
        {
            var response = await _employeeService.GetEmployees(branchId);
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
        [HttpGet("getEmployeeByEmail")]
        public async Task<IActionResult> GetEmployeeByEmail([FromQuery] string branchId, string email)
        {
            if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { Message = "Branch ID and Email are required." });
            }

            var response = await _employeeService.GetEmployeeByEmail(branchId, email);

            if (response.Success)
            {
                return Ok(response.Data); // คืนค่าพนักงานในรูปแบบ JSON
            }

            return NotFound(response.Message); // ส่งคืนข้อความถ้าไม่พบพนักงาน
        }

        [HttpGet("branches/{branchId}/employees/{employeeId}")]
        public async Task<IActionResult> GetEmployeeById(string branchId, string employeeId)
        {
            var result = await _employeeService.GetEmployeeById(branchId, employeeId);
            if (result.Success)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }


        [CustomAuthorizeRole("Admin, Manager")]
        [HttpPut("branches/{branchId}/employees/{employeeId}")]
        public async Task<IActionResult> UpdateEmployee(string branchId, string employeeId, [FromBody] Employee updatedEmployee)
        {
            try
            {
                // ตรวจสอบข้อมูลที่จำเป็น
                if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(employeeId) || updatedEmployee == null)
                {
                    return BadRequest(new { Success = false, Message = "BranchId, EmployeeId, and Employee object cannot be null or empty." });
                }

                // ดึงข้อมูลพนักงานเดิมจากฐานข้อมูล
                var employeeResponse = await _employeeService.UpdateEmployee(branchId, employeeId, updatedEmployee);
                if (!employeeResponse.Success)
                {
                    return NotFound(new { Success = false, Message = employeeResponse.Message });
                }

                return Ok(new { Success = true, Message = employeeResponse.Message });
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error updating employee: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = $"Failed to update employee: {ex.Message}" });
            }
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpDelete("branches/{branchId}/employees/{employeeId}")]
        public async Task<IActionResult> DeleteEmployee(string branchId, string employeeId)
        {
            try
            {
                await _employeeService.DeleteEmployee(branchId, employeeId);
                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [CustomAuthorizeRole("Admin")]
        [HttpDelete("deleteall/{branchId}")]
        public async Task<IActionResult> DeleteAllEmployees(string branchId)
        {
            // ตรวจสอบว่า branchId ไม่เป็น null หรือว่างเปล่า
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return BadRequest("Branch ID cannot be null or empty.");
            }

            try
            {
                var response = await _employeeService.DeleteAllEmployees(branchId);

                if (response.Success)
                {
                    return Ok(response.Message); // ส่งค่าความสำเร็จกลับ
                }

                return BadRequest(response.Message); // ส่งค่าข้อผิดพลาดกลับ
            }
            catch (Exception ex)
            {
                // บันทึกข้อผิดพลาด
                // Logger.LogError(ex, "Error deleting employees for branch {branchId}", branchId); // ใช้ logging ตามที่ได้ตั้งไว้
                return StatusCode(500, "An unexpected error occurred. Please try again later."); // ข้อผิดพลาดที่ไม่คาดคิด
            }
        }

        [CustomAuthorizeRole("Admin")]
        [HttpPost("reset-employee-sequence")]
        public async Task<IActionResult> ResetEmployeeId(string branchId)
        {
            var response = await _employeeService.ResetEmployeeId(branchId);

            if (response.Success)
            {
                return Ok(new { Success = true, Message = response.Message });
            }

            return BadRequest(new { Success = false, Message = response.Message });
        }

        [CustomAuthorizeRole("Admin, Manager")]
        [HttpGet("get-employee-by-firstname")]
        public async Task<IActionResult> GetEmployeeByFirstName(string branchId, string firstName)
        {
            var response = await _employeeService.GetEmployeeByFirstName(branchId, firstName);
            if (response.Success) return Ok(response.Data);
            return BadRequest(response);
        }
    }
}