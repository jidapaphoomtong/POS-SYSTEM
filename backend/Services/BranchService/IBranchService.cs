using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.BranchService
{
    public interface IBranchService
    {
        string GenerateSalt();
        bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt);
        string HashPassword(string password, string salt);
        Task<bool> IsEmailRegistered(string email);
        Task<string> GetNextId(string sequenceName);
        Task<ServiceResponse<string>> AddBranch(Branch branch);
        Task<ServiceResponse<List<BranchResponse>>> GetBranches();
        Task<ServiceResponse<BranchResponse>> GetBranchById(string branchId);
        Task<ServiceResponse<string>> UpdateBranch(string branchId, Branch branch);
        Task<bool> UpdateBranchStatusAsync(string branchId, string status);
        Task<ServiceResponse<string>> DeleteBranch(string branchId);
    }
}