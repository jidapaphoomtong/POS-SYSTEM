using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductController : Controller
    {
        private readonly FirestoreDB _firestoreDb;

        public ProductController (FirestoreDB firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
    }
    
    // [HttpPost(add-product)]
    // public async Task<IActionResult> AddProduct([FromBody] AddProduct addProduct){

    // }
    //     var productController = _firestoreDb.Collections("products");
    //     var newProduct = await productController.AddProductAsync(new{
            
    //     })
}