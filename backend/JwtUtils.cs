using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace backend
{
    public class JwtUtils
    {
        private readonly string _secretKey;

    public JwtUtils(string secretKey)
    {
        _secretKey = secretKey;
    }

    public string ModifyToken(string token, Dictionary<string, object> updatedClaims)
    {
        // ถอดรหัสส่วน Payload
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // แปลง Payload ที่ได้รับจาก Token เป็น Dictionary
        var existingClaims = jwtToken.Claims.ToDictionary(claim => claim.Type, claim => (object)claim.Value);

        // อัปเดต Claims โดยเพิ่ม/แทนที่ค่าใน Payload เดิม
        foreach (var claim in updatedClaims)
        {
            existingClaims[claim.Key] = claim.Value;
        }

        // สร้าง Claim ใหม่หลังจากปรับค่า Claims
        var claims = existingClaims.Select(pair => new Claim(pair.Key, pair.Value.ToString())).ToList();

        // สร้าง Security Key จาก Secret Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // สร้าง Token ใหม่
        var newJwtToken = new JwtSecurityToken(
            issuer: jwtToken.Issuer,
            audience: jwtToken.Audiences.FirstOrDefault(),
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: jwtToken.ValidTo, // ใช้เวลาหมดอายุเดิม
            signingCredentials: credentials
        );

        // เข้ารหัสและคืนเป็น Token String
        return handler.WriteToken(newJwtToken);
    }
    }
}