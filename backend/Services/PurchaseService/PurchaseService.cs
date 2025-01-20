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
                var sequenceDoc = _firestoreDb.Collection("config").Document(sequenceName);
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

                var purchaseDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("purchases")
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
                var purchasesQuery = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("purchases");

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var purchases = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                return ServiceResponse<IEnumerable<Purchase>>.CreateSuccess(purchases, "Purchases retrieved successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Purchase>>.CreateFailure($"Failed to fetch purchases: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<Purchase>>> GetMonthlySales(string branchId, int year, int month)
        {
            try
            {
                var purchasesQuery = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("purchases")
                    .WhereGreaterThan("Date", new DateTime(year, month, 1))
                    .WhereLessThan("Date", new DateTime(year, month + 1, 1));

                var snapshot = await purchasesQuery.GetSnapshotAsync();
                var monthlySales = snapshot.Documents.Select(doc => doc.ConvertTo<Purchase>()).ToList();

                return ServiceResponse<IEnumerable<Purchase>>.CreateSuccess(monthlySales, "Monthly sales retrieved successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<Purchase>>.CreateFailure($"Failed to fetch monthly sales: {ex.Message}");
            }
        }
        
        public async Task<ServiceResponse<bool>> DeleteAllPurchases(string branchId)
        {
            try
            {
                var purchasesQuery = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("purchases");

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