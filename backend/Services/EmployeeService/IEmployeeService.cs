using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.EmployeeService
{
    public interface IEmployeeService
    {
        string GenerateSalt();
        bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt);
        string HashPassword(string password, string salt);
        Task<bool> IsEmailRegistered(string email);
        Task<string> GetNextId(string sequenceName);
        Task<ServiceResponse<string>> AddEmployee(string branchId, Employee employee);
        Task<ServiceResponse<List<object>>> GetEmployees(string branchId);
        Task<ServiceResponse<Employee>> GetEmployeeByEmail(string branchId ,string email);
        Task<ServiceResponse<Employee>> GetEmployeeById(string branchId, string employeeId);
        Task<ServiceResponse<string>> UpdateEmployee(string branchId, string employeeId, Employee updatedEmployee);
        Task<ServiceResponse<string>> DeleteEmployee(string branchId, string employeeId);
        Task<ServiceResponse<string>> DeleteAllEmployees(string branchId);
        Task<ServiceResponse<string>> ResetEmployeeId(string branchId);
        Task<ServiceResponse<List<Employee>>> GetEmployeeByFirstName(string branchId, string firstName);
    }
}