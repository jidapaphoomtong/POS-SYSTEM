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
        Task<ServiceResponse<Purchase>> GetPurchaseById(string branchId, string purchaseId);
        Task<ServiceResponse<IEnumerable<Purchase>>> GetMonthlySales(string branchId, int year, int month);
        Task<ServiceResponse<bool>> DeleteAllPurchases(string branchId);
    }
}