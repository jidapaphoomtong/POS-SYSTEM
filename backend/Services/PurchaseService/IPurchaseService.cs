using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.PurchaseService
{
    public interface IPurchaseService
    {
        Task<string> GetNextId(string sequenceName);
        Task<ServiceResponse<string>> AddPurchase(string branchId, Purchase purchase);
    }
}