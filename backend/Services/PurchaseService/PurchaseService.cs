using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Google.Cloud.Firestore;

namespace backend.Services.PurchaseService
{
    public class PurchaseService : IPurchaseService
    {
        private readonly FirestoreDB _firestoreDb;

        public PurchaseService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<string> GetNextId(string sequenceName)
        {
            try
            {
                var sequenceDoc = _firestoreDb.Collection(FirestoreCollections.Config).Document(sequenceName);
                var snapshot = await sequenceDoc.GetSnapshotAsync();

                int counter = snapshot.Exists && snapshot.TryGetValue("counter", out int currentCounter)
                    ? currentCounter + 1
                    : 1;

                await sequenceDoc.SetAsync(new { counter });
                return counter.ToString("D3");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate ID for {sequenceName}: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> AddPurchase(string branchId, Purchase purchase)
        {
            try
            {
                string purchaseId = await GetNextId($"purchase-sequence-{branchId}");
                purchase.Id = purchaseId;

                if (purchase == null)
                {
                    return ServiceResponse<string>.CreateFailure("Purchase data is null.");
                }

                //ดึงข้อมูล product : price และคำนวณ

                var purchaseDoc = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases)
                    .Document(purchaseId);

                await purchaseDoc.SetAsync(purchase);
                return ServiceResponse<string>.CreateSuccess(purchaseId, "Purchase Data added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add purchase data: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<Purchase>>> GetAllPurchases(string branchId)
        {
            try
            {
                var purchasesQuery = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases);

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var purchases = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                if (purchases.Count == 0)
                {
                    return ServiceResponse<IEnumerable<Purchase>>.CreateFailure("Don't have purchase data in this branch.");
                }

                return ServiceResponse<IEnumerable<Purchase>>.CreateSuccess(purchases, "Purchases retrieved successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Purchase>>.CreateFailure($"Failed to fetch purchases: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Purchase>> GetPurchaseById(string branchId, string purchaseId)
        {
            try
            {
                // เข้าถึงคำสั่งซื้อเฉพาะจาก Firestore
                var purchaseDocRef = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases)
                    .Document(purchaseId);

                var snapshot = await purchaseDocRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    // แปลงข้อมูลใน snapshot เป็น Purchase
                    var purchase = snapshot.ConvertTo<Purchase>();
                    return ServiceResponse<Purchase>.CreateSuccess(purchase, "Purchase retrieved successfully!");
                }
                else
                {
                    return ServiceResponse<Purchase>.CreateFailure("Purchase not found.");
                }
            }
            catch (Exception ex)
            {
                return ServiceResponse<Purchase>.CreateFailure($"Failed to fetch purchase: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<SalesSummaryDto>> GetSalesSummary(string branchId)
        {
            // ดึงข้อมูลและคำนวณสรุปยอดขาย
            try
            {
                var purchasesQuery = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases);

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var purchases = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                if (!purchases.Any())
                {
                    return ServiceResponse<SalesSummaryDto>.CreateFailure("Don't have purchase data in this branch.");
                }

                var totalSales = purchases.Sum(p => p.Total);
                var totalTransactions = purchases.Count;

                var dailySales = purchases
                    .GroupBy(p => p.Date.ToString("yyyy-MM-dd"))
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key,
                        Amount = g.Sum(p => p.Total),
                        TransactionCount = g.Count(),
                        AveragePerTransaction = g.Sum(p => p.Total) / (g.Count() > 0 ? g.Count() : 1)
                    })
                    .ToList();

                var summary = new SalesSummaryDto
                {
                    TotalSales = totalSales,
                    TotalTransactions = totalTransactions,
                    DailySales = dailySales
                };

                return ServiceResponse<SalesSummaryDto>.CreateSuccess(summary, "สรุปยอดขายได้รับแล้ว!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<SalesSummaryDto>.CreateFailure($"Failed to fetch purchase: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<DailySalesDto>>> GetDailySalesSummary(string branchId, int year, int month, int day)
        {
            // ดึงข้อมูลยอดขายรายวัน
            try
            {
                var purchasesQuery = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases)
                    .WhereEqualTo("Year", year)
                    .WhereEqualTo("Month", month)
                    .WhereEqualTo("Day", day);

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var purchases = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                if (!purchases.Any())
                {
                    return ServiceResponse<List<DailySalesDto>>.CreateFailure("Don't have purchase data in this branch.");
                }

                var dailySales = purchases
                    .GroupBy(p => p.Date.ToString("yyyy-MM-dd"))
                    .Select(g => new DailySalesDto
                    {
                        Date = g.Key,
                        Amount = g.Sum(p => p.Total),
                        TransactionCount = g.Count(),
                        AveragePerTransaction = g.Sum(p => p.Total) / (g.Count() > 0 ? g.Count() : 1)
                    })
                    .ToList();

                return ServiceResponse<List<DailySalesDto>>.CreateSuccess(dailySales, "สรุปยอดขายได้รับแล้ว!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<DailySalesDto>>.CreateFailure($"Failed to fetch purchase: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<Purchase>>> GetMonthlySales(string branchId, int year, int month)
        {
            try
            {
                // สร้างวันที่เริ่มต้นและสิ้นสุดในรูปแบบ UTC
                var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = new DateTime(year, month + 1, 1, 0, 0, 0, DateTimeKind.Utc);
                
                var purchasesQuery = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases)
                    .WhereGreaterThan("Date", startDate)
                    .WhereLessThan("Date", endDate);

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var monthlySales = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                return ServiceResponse<IEnumerable<Purchase>>.CreateSuccess(monthlySales, "ดึงข้อมูลยอดขายรายเดือนได้สำเร็จ!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Purchase>>.CreateFailure($"Failed to fetch purchase: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<Purchase>>> GetYearlySales(string branchId, int year)
        {
            try
            {
                // สร้างวันที่เริ่มต้นและสิ้นสุดในรูปแบบ UTC
                var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                var purchasesQuery = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases)
                    .WhereGreaterThan("Date", startDate)
                    .WhereLessThan("Date", endDate);

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var yearlySales = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                return ServiceResponse<IEnumerable<Purchase>>.CreateSuccess(yearlySales, "ดึงข้อมูลยอดขายรายปีได้สำเร็จ!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Purchase>>.CreateFailure($"Failed to fetch purchase: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<Purchase>>> GetSalesByEmployee(string branchId, string employeeId)
        {
            try
            {
                var purchasesQuery = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases)
                    .WhereEqualTo("Seller", employeeId); // สมมุติว่า field ของพนักงานคือ "Seller"

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var employeeSales = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                if (!employeeSales.Any())
                {
                    return ServiceResponse<IEnumerable<Purchase>>.CreateFailure("No sales found for this employee.");
                }

                return ServiceResponse<IEnumerable<Purchase>>.CreateSuccess(employeeSales, "ดึงข้อมูลยอดขายโดยพนักงานได้สำเร็จ!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Purchase>>.CreateFailure($"Failed to fetch employee sales: {ex.Message}");
            }
        }
        
        public async Task<ServiceResponse<bool>> DeleteAllPurchases(string branchId)
        {
            try
            {
                var purchasesQuery = _firestoreDb
                    .Collection(FirestoreCollections.Branches)
                    .Document(branchId)
                    .Collection(FirestoreCollections.Purchases);

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                if (snapshot.Documents.Count == 0)
                {
                    return ServiceResponse<bool>.CreateFailure("No purchases to delete.");
                }

                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.DeleteAsync();
                }

                // รีเซต purchaseId
                await ResetPurchaseId(branchId);

                return ServiceResponse<bool>.CreateSuccess(true, "All purchases deleted successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.CreateFailure($"Failed to delete purchases: {ex.Message}");
            }
        }

        private async Task ResetPurchaseId(string branchId)
        {
            var sequenceDoc = _firestoreDb.Collection("config").Document($"purchase-sequence-{branchId}");
            await sequenceDoc.SetAsync(new { counter = 0 });
        }
    }
}