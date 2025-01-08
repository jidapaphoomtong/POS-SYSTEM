using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Services.Tokenservice
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<(string newAccessToken, string newRefreshToken)> RefreshToken(string token, string refreshToken, FirestoreDB firestoreDb); // ใช้สำหรับ Refresh Token
        Task RevokeToken(string userId, FirestoreDB firestoreDb); // ใช้เพื่อลบ Refresh Token
        ClaimsPrincipal? ValidateToken(string authToken);
    }
}