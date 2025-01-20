using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.PurchaseService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [DisableCors]
    [LogAction]
    public class PurchaseController : Controller
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
            var response = await _purchaseService.AddPurchase(branchId, purchase);
            if (response.Success) return Ok(response);
            return BadRequest(response);
        }
    }
}