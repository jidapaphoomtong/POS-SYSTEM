using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Models;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.Tokenservice
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly ILogger<TokenService> _logger;

        // Constructor: Inject configuration settings and logger
        public TokenService(IOptions<JwtSettings> settings, ILogger<TokenService> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate a new Access Token (JWT)
        /// </summary>
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            try
            {
                // Create the secret key
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

                // Create the token
                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenDurationInMinutes),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                // Write token string
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while generating access token: {Message}", ex.Message);
                throw new ApplicationException("Failed to generate access token", ex);
            }
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token is empty or null.");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // ไม่มีการยืดเวลา
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token has expired.");
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning($"Invalid token: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error during token validation: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Generate a secure Refresh Token
        /// </summary>
        public string GenerateRefreshToken()
        {
            try
            {
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }
                return Convert.ToBase64String(randomNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while generating refresh token: {Message}", ex.Message);
                throw new ApplicationException("Failed to generate refresh token", ex);
            }
        }

        /// <summary>
        /// Validate and retrieve claims from an expired token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>ClaimsPrincipal containing token claims</returns>
        /// <exception cref="SecurityTokenException">Thrown if token is invalid</exception>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                // Create the security key
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

                // Token validation parameters
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = false // Allow expired tokens
                };

                // Validate the token
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                // Ensure the token is a valid JWT
                if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error validating expired token: {Message}", ex.Message);
                throw new SecurityTokenException("Failed to validate expired token", ex);
            }
        }

        public async Task<(string newAccessToken, string newRefreshToken)> RefreshToken(string token, string refreshToken, FirestoreDB firestoreDb)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var userId = principal.Identity?.Name;

            // Get user document
            var userSnapshot = await firestoreDb.Collection(FirestoreCollections.Users).Document(userId).GetSnapshotAsync();
            if (!userSnapshot.Exists)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Validate refreshToken
            var storedRefreshToken = userSnapshot.GetValue<string>("refreshToken");
            if (storedRefreshToken != refreshToken)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();

            // Update Firestore with the new refresh token
            var updateData = new Dictionary<string, object> { { "refreshToken", newRefreshToken } };
            await userSnapshot.Reference.UpdateAsync(updateData);

            return (newAccessToken, newRefreshToken);
        }

        public async Task RevokeToken(string userId, FirestoreDB firestoreDb)
        {
            // Get user document
            var userSnapshot = await firestoreDb.Collection(FirestoreCollections.Users).Document(userId).GetSnapshotAsync();
            if (!userSnapshot.Exists)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Remove refreshToken
            var updateData = new Dictionary<string, object> { { "refreshToken", FieldValue.Delete } };
            await userSnapshot.Reference.UpdateAsync(updateData);
        }
    }
}