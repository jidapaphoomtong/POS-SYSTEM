using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class JwtTokenHelper
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenHelper(string secretKey, string issuer, string audience)
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateJwtToken(string id,string email, string firstName, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        // Claims (ข้อมูลใน Payload)
        var claims = new List<Claim>
        {
            // new Claim(JwtRegisteredClaimNames.Sub, subject), // sub
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // jti
            new Claim("id", id), // id
            new Claim("name", firstName),
            new Claim("email", email),
            new Claim("role",role),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64) // iat
        };

        // สร้าง Token
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow, // nbf
            expires: DateTime.UtcNow.AddMinutes(15), // exp
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    //กำหนด role ใน token
    // private string GenerateJwtToken(string role)
    // {
    //     var claims = new[]
    //     {
    //         new Claim(ClaimTypes.Name, "Username"),
    //         new Claim(ClaimTypes.Role, role), // เพิ่ม Role
    //     };

    //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
    //     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //     var token = new JwtSecurityToken(
    //         issuer: "localhost",
    //         audience: "localhost",
    //         claims: claims,
    //         expires: DateTime.Now.AddHours(1),
    //         signingCredentials: creds);

    //     return new JwtSecurityTokenHandler().WriteToken(token);
    // }
}