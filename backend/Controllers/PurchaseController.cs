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
    }
}