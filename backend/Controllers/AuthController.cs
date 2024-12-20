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
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] Login userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Example logic for email or PIN login
            if (!string.IsNullOrEmpty(userLogin.Email))
            {
                // Handle login with Email
                return Ok("Login successful via Email!");
            }
            else if (!string.IsNullOrEmpty(userLogin.PIN))
            {
                // Handle login with PIN
                return Ok("Login successful via PIN!");
            }

            return BadRequest("Invalid login request.");
        }
    }
}