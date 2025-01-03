using System;
using System.Threading.Tasks;
using backend.Services.AuthService;
using backend.Services.Tokenservice;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    
    [Route("API/[controller]")]
    [Authorize]
    public class TokenController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;

        public TokenController(ITokenService tokenService, IAuthService authService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("api/token/refresh")]
        public async Task<IActionResult> Refresh(string token, string refreshToken)
        {
            // Validate `token` and `refreshToken` inputs
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("Invalid token or refresh token.");
            }

            // Decode and extract claims from the expired token
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);
            var userId = principal?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("Invalid token or user ID.");
            }

            // Retrieve user document from Firestore
            var userSnapshot = await _authService.GetUserById(userId);
            if (userSnapshot == null)
            {
                return Unauthorized("User not found.");
            }

            // Check if refreshToken matches
            var storedRefreshToken = userSnapshot.GetValue<string>("refreshToken");
            if (storedRefreshToken != refreshToken)
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update Firestore with the new refresh token
            var updateData = new Dictionary<string, object>
            {
                { "refreshToken", newRefreshToken }
            };
            await userSnapshot.Reference.UpdateAsync(updateData);

            // Return the new tokens in the response
            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        [Authorize]
        [HttpPost("api/token/revoke")]
        public async Task<IActionResult> Revoke()
        {
            var userId = User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("Invalid user ID.");
            }

            // Retrieve the user document from Firestore
            var userSnapshot = await _authService.GetUserById(userId);
            if (userSnapshot == null)
            {
                return NotFound("User not found.");
            }

            // Set `refreshToken` to null in Firestore
            var updateData = new Dictionary<string, object>
            {
                { "refreshToken", FieldValue.Delete } // Clears the field instead of setting to null
            };
            await userSnapshot.Reference.UpdateAsync(updateData);

            return NoContent();
        }
    }
}