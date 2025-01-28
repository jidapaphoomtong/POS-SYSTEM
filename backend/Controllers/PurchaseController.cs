using System.Threading.Tasks;
using backend.Models;
using backend.Services.PurchaseService;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [LogAction]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpPost("add-purchase/{branchId}")]
        public async Task<IActionResult> AddPurchase(string branchId, [FromBody] Purchase purchase)
        {
            if (purchase == null)
            {
                return BadRequest("Invalid purchase data.");
            }

            var response = await _purchaseService.AddPurchase(branchId, purchase);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("all-purchases/{branchId}")]
        public async Task<IActionResult> GetAllPurchases(string branchId)
        {
            var response = await _purchaseService.GetAllPurchases(branchId);
            if (response.Success)
                return Ok(response.Data);

            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("branches/{branchId}/purchases/{purchaseId}")]
        public async Task<IActionResult> GetPurchaseById(string branchId, string purchaseId)
        {
            var response = await _purchaseService.GetPurchaseById(branchId, purchaseId);

            if (response.Success)
            {
                return Ok(response.Data);
            }

            return NotFound(new { message = response.Message });
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("monthly-sales/{branchId}/{year}/{month}")]
        public async Task<IActionResult> GetMonthlySales(string branchId, int year, int month)
        {
            var response = await _purchaseService.GetMonthlySales(branchId, year, month);
            if (response.Success)
                return Ok(response.Data);

            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpDelete("delete-all-purchases/{branchId}")]
        public async Task<IActionResult> DeleteAllPurchases(string branchId)
        {
            var response = await _purchaseService.DeleteAllPurchases(branchId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("sales-summary/{branchId}")]
        public async Task<ActionResult<ServiceResponse<SalesSummaryDto>>> GetSalesSummary(string branchId)
        {
            var response = await _purchaseService.GetSalesSummary(branchId);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("daily-sales-summary/{branchId}")]
        public async Task<ActionResult<ServiceResponse<List<DailySalesDto>>>> GetDailySalesSummary(string branchId, int year, int month, int day)
        {
            var response = await _purchaseService.GetDailySalesSummary(branchId, year, month, day);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(response.Message);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("yearly-sales/{branchId}/{year}")]
        public async Task<IActionResult> GetYearlySales(string branchId, int year)
        {
            var response = await _purchaseService.GetYearlySales(branchId, year);
            if (response.Success)
                return Ok(response.Data);
            return BadRequest(response);
        }

        [CustomAuthorizeRole("Admin, Manager, Employee")]
        [HttpGet("employee-sales/{branchId}/{employeeId}")]
        public async Task<IActionResult> GetSalesByEmployee(string branchId, string employeeId)
        {
            var response = await _purchaseService.GetSalesByEmployee(branchId, employeeId);
            if (response.Success)
                return Ok(response.Data);
            return BadRequest(response);
        }
    }
}