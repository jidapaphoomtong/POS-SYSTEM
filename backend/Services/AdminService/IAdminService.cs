// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using backend.Models;

// namespace backend.Services.AdminService
// {
//     public interface IAdminService
//     {
//         string GenerateSalt();
//         bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt);
//         string HashPassword(string password, string salt);
//         Task<bool> IsEmailRegistered(string email);
//         Task<string> GetNextId(string sequenceName);
//         Task<ServiceResponse<string>> AddBranch(Branch branch);
//         Task<ServiceResponse<string>> AddEmployee(string branchId, Employee employee);
//         Task<ServiceResponse<string>> AddProduct(string branchId, Products product);
//         Task<ServiceResponse<List<BranchResponse>>> GetBranches();
//         Task<ServiceResponse<string>> UpdateBranch(string branchId, Branch branch);
//         Task<ServiceResponse<string>> DeleteBranch(string branchId);
//         Task<ServiceResponse<List<object>>> GetEmployees(string branchId);
//         Task<ServiceResponse<Employee>> GetEmployeeByEmail(string branchId ,string email);
//         Task<ServiceResponse<string>> UpdateEmployee(string branchId, string employeeId, Employee updatedEmployee);
//         Task<ServiceResponse<string>> DeleteEmployee(string branchId, string employeeId);
//         Task<ServiceResponse<List<object>>> GetProducts(string branchId);
//         Task<ServiceResponse<string>> UpdateProduct(string branchId, string productId, Products updatedProduct);
//         Task<ServiceResponse<string>> DeleteProduct(string branchId, string productId);
//         Task<ServiceResponse<BranchResponse>> GetBranchById(string branchId);
//         Task<ServiceResponse<string>> DeleteAllEmployees(string branchId);
//         Task<ServiceResponse<string>> ResetEmployeeId(string branchId);
//     }
// }