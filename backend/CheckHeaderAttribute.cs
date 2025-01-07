using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Filters
{
    public class CheckHeaderAttribute : ActionFilterAttribute
    {
        private readonly string _headerName = "x-posapp-header";
        private readonly string _expectedValue;
        private readonly JwtSettings _settings;
        private readonly ILogger<CheckHeaderAttribute> _logger;

        public CheckHeaderAttribute(IConfiguration configuration, JwtSettings settings, ILogger<CheckHeaderAttribute> logger)
        {
            _expectedValue = configuration["ApiSettings:HeaderSecretKey"] ?? throw new ArgumentNullException("ApiSettings:HeaderSecretKey");
            _logger = logger;
            _settings = settings;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            var cookies = headers["Cookie"].ToString();

            if (!string.IsNullOrEmpty(cookies))
            {
                var cookiesDictionary = cookies.Split(';')
                                                .Select(c => c.Split('='))
                                                .Where(c => c.Length == 2)
                                                .ToDictionary(c => c[0].Trim(), c => c[1].Trim());
                
                if (cookiesDictionary.TryGetValue("authToken", out var authToken))
                {
                    _logger.LogInformation($"AuthToken: {authToken}");

                    // สร้าง instance ของ JwtUtils
                    var jwtUtils = new JwtUtils(_settings.SecretKey);
                    
                    // อัปเดต Claims ที่คุณต้องการ
                    var updatedClaims = new Dictionary<string, object>
                    {
                        { "newClaimKey", "newClaimValue" } // เพิ่มหรือปรับแก้ค่าใน claims ที่คุณต้องการ
                    };

                    // โมดิฟาย JWT token
                    var modifiedToken = jwtUtils.ModifyToken(authToken, updatedClaims);
                    _logger.LogInformation($"Modified AuthToken: {modifiedToken}");

                    // หากต้องการใช้ token ใหม่ คุณสามารถเก็บมันในคุกกี้หรือ headers
                    // context.HttpContext.Response.Cookies.Append("authToken", modifiedToken);
                }
                else
                {
                    _logger.LogWarning("AuthToken not found in cookies.");
                }
            }
            else
            {
                _logger.LogWarning("No cookies found in the request headers.");
            }

            var endpointMeta = context.HttpContext.GetEndpoint()?.Metadata;

            // ถ้ามี [AllowAnonymous] ให้ข้ามการตรวจ Header
            if (endpointMeta != null && endpointMeta.GetMetadata<IAllowAnonymous>() != null)
            {
                base.OnActionExecuting(context); // ข้ามการตรวจสอบ Header
                return;
            }

            // ตรวจสอบ Header ว่ามี "x-posapp-header" หรือไม่
            if (!headers.ContainsKey(_headerName) || headers[_headerName] != _expectedValue)
            {
                // ออกจากระบบ (SignOut)
                context.HttpContext.SignOutAsync(); // Clear Authentication

                // คืนการตอบกลับ 401 Unauthorized
                context.Result = new UnauthorizedObjectResult(new
                {
                    Success = false,
                    Message = "Invalid or missing required header. You have been logged out."
                });

                return; // หยุดการทำงาน
            }

            base.OnActionExecuting(context);
        }
    }
}