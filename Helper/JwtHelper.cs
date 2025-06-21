﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using dotnet9_jwt_concept.Models.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dotnet9_jwt_concept.Core;
using Octokit;
using User = dotnet9_jwt_concept.Core.User;

namespace dotnet9_jwt_concept.Helper
{
    public class JwtHelper
    {
        private readonly JwtSettings _jwt;
        public JwtHelper(IOptions<AppSettings> appSettings)
        {
            _jwt = appSettings.Value.Jwt;
        }

        public string GenerateToken(User payload)
        {
            // 1. สร้าง Security Key และ Credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. กำหนด Claims ที่ต้องการฝังในโทเค็น
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, payload.user_name),
                new Claim("userId", payload.user_id.ToString()),
                new Claim("userName", payload.user_name),
                new Claim("firstName", payload.user_fname),
                new Claim("lastName", payload.user_lname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 3. สร้าง Token Descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiration),
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                SigningCredentials = creds
            };

            // 4. สร้างและเขียนเป็นสตริง
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public  ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateIssuer = true,
                ValidIssuer = _jwt.Issuer,

                ValidateAudience = true,
                ValidAudience = _jwt.Audience,

                ValidateLifetime = true,            // ตรวจสอบเวลา Expire
                ClockSkew = TimeSpan.Zero           // ไม่ให้บวกลบเวลาเผื่อ
            };

            // 4. Validate และคืน ClaimsPrincipal
            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            return principal;
        }

        public User? DecodeUserFromToken(HttpContext context)
        {
            // 1. ดึง JWT จาก header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // 2. Validate token (ปลอดภัยสุด)
            try
            {
                var principal = ValidateToken(token);
                var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);

                return new User
                {
                    user_id = int.TryParse(claims.GetValueOrDefault("userId"), out var uid) ? uid : 0,
                    user_name = claims.GetValueOrDefault("userName") ?? "",
                    user_fname = claims.GetValueOrDefault("firstName") ?? "",
                    user_lname = claims.GetValueOrDefault("lastName") ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

    }
}
