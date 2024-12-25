using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class JwtTokenHelper
{
    private const string SecretKey = "wrZzjoEgLiypg53ojlxG"; // Secret Key
    private const string Issuer = "localhost"; // Issuer
    private const string Audience = "localhost"; // Audience

    public static string GenerateJwtToken(string id, string subject, List<string> roles)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        // Claims (ข้อมูลใน Payload)
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject), // sub
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // jti
            new Claim("id", id), // id
            new Claim("given_name", "Super"), // ให้ fixed string
            new Claim("family_name", "User"),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64), // iat
        };

        // เพิ่ม Role Claim
        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        // สร้าง Token
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: DateTime.UtcNow, // nbf
            expires: DateTime.UtcNow.AddMinutes(15), // exp
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}