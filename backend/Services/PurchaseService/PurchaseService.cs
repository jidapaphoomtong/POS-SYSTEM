using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

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

                // Increment ลำดับ
                await sequenceDoc.SetAsync(new { counter = counter + 1 });

                // คืนค่า ID ในรูปแบบ "001", "002", "003"
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
                // สร้าง purchase ID ใหม่
                string purchaseId = await GetNextId($"purchase-sequence-{branchId}"); // ลำดับเฉพาะต่อ Branch

                var purchaseDoc = _firestoreDb.Collection("branches")
                    .Document(branchId)
                    .Collection("purchases")
                    .Document(purchaseId);

                purchase.Id = purchaseId;
                await purchaseDoc.SetAsync(purchase);

                return ServiceResponse<string>.CreateSuccess(purchaseId, "Purchase Data added successfully!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.CreateFailure($"Failed to add purchase data: {ex.Message}");
            }
        }
    }
}