using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly FirestoreDB _firestoreDb;

        public AdminService(FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        //ตัวอย่างโครงสร้างในการสร้าง subcollection
        // เพิ่ม Branch เข้า Collection "branches"
        // [HttpPost("add-branch")]
        // public async Task<IActionResult> AddBranch([FromBody] Branch branch)
        // {
        //     try
        //     {
        //         // สร้าง Document ใหม่ใน Collection "branches"
        //         CollectionReference branches = _firestoreDb.Collection("branches");
        //         DocumentReference newBranch = await branches.AddAsync(branch);

        //         return Ok(new { message = "Branch added successfully", documentId = newBranch.Id });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // // เพิ่ม Employee เข้า Subcollection "employees" ใน Branch (Dynamic Branch)
        // [HttpPost("add-employee/{branchId}")]
        // public async Task<IActionResult> AddEmployee(string branchId, [FromBody] Employee employee)
        // {
        //     try
        //     {
        //         CollectionReference employees = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("employees");

        //         DocumentReference newEmployee = await employees.AddAsync(employee);

        //         return Ok(new { message = "Employee added successfully", documentId = newEmployee.Id });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        // // Dynamic Example: กำหนด Product เข้า Subcollection "products"
        // [HttpPost("add-product/{branchId}")]
        // public async Task<IActionResult> AddProduct(string branchId, [FromBody] Product product)
        // {
        //     try
        //     {
        //         CollectionReference products = _firestoreDb
        //             .Collection("branches")
        //             .Document(branchId)
        //             .Collection("products");

        //         DocumentReference newProduct = await products.AddAsync(product);

        //         return Ok(new { message = "Product added successfully", documentId = newProduct.Id });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }
    }
}