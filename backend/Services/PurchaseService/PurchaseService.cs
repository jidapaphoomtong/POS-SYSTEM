using System.Collections.Generic;
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

                int counter = 1;

                if (snapshot.Exists && snapshot.TryGetValue<int>("counter", out var currentCounter))
                {
                    counter = currentCounter;
                }

                // Increment the sequence
                await sequenceDoc.SetAsync(new { counter = counter + 1 });

                // Return ID formatted
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
    }
}