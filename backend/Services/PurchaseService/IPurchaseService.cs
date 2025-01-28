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
        Task<ServiceResponse<SalesSummaryDto>> GetSalesSummary(string branchId);
        Task<ServiceResponse<IEnumerable<Purchase>>> GetMonthlySales(string branchId, int year, int month);
        Task<ServiceResponse<IEnumerable<Purchase>>> GetYearlySales(string branchId, int year);
        Task<ServiceResponse<IEnumerable<Purchase>>> GetSalesByEmployee(string branchId, string employeeId);
        Task<ServiceResponse<bool>> DeleteAllPurchases(string branchId);
        Task<ServiceResponse<List<DailySalesDto>>> GetDailySalesSummary(string branchId, int year, int month, int day);
    }
}