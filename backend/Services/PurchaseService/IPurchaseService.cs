using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.PurchaseService
{
    public interface IPurchaseService
    {
        Task<string> GetNextId(string sequenceName);
        Task<ServiceResponse<string>> AddPurchase(string branchId, Purchase purchase);
        Task<ServiceResponse<IEnumerable<Purchase>>> GetAllPurchases(string branchId);
        Task<ServiceResponse<IEnumerable<Purchase>>> GetMonthlySales(string branchId, int year, int month);
    }
}