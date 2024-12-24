using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly FirestoreDB _firestoreDb;
        public ProductController(FirestoreDB firestoreDB)
        {
            _firestoreDb = firestoreDB;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] Products product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var productCollection = _firestoreDb.Collection("products");
                
                var newProduct = new
                {
                    product.Title,
                    product.Quantity,
                    Price = (double)product.Price
                };
                
                var addedProduct = await productCollection.AddAsync(newProduct);

                return Ok(new { Success = true, Message = "Product added successfully!", ProductId = addedProduct.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while adding the product. " + ex.Message });
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var productCollection = _firestoreDb.Collection("products");
                var snapshot = await productCollection.GetSnapshotAsync();

                if (!snapshot.Documents.Any())
                {
                    return NotFound(new { Success = false, Message = "No products found in Firestore." });
                }

                var products = snapshot.Documents.Select(doc => new
                {
                    Id = doc.Id,
                    Fields = doc.ToDictionary() 
                }).ToList();

                return Ok(new { Success = true, Products = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred while fetching products: {ex.Message}" });
            }
        }

        [HttpPut("update/{documentId}")]
        public async Task<IActionResult> UpdateProduct(string documentId, [FromBody] Products product)
        {
            try
            {
                var docRef = _firestoreDb.Collection("products").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                    return NotFound(new { Success = false, Message = "Product not found." });

                var updateData = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(product.Title))
                    updateData["Title"] = product.Title;

                if (product.Quantity > 0)
                    updateData["Quantity"] = product.Quantity;

                if (product.Price > 0)
                    updateData["Price"] = product.Price;

                if (updateData.Count == 0)
                {
                    return BadRequest(new { Success = false, Message = "No valid fields specified for update." });
                }

                await docRef.UpdateAsync(updateData);
                return Ok(new { Success = true, Message = "Product updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = $"Error updating product: {ex.Message}" });
            }
        }

        [HttpDelete("delete/{documentId}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] string documentId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("products").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                    return NotFound(new { Success = false, Message = "Product not found." });

                await docRef.DeleteAsync();
                return Ok(new { Success = true, Message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = $"Error deleting product: {ex.Message}" });
            }
        }
    }
}