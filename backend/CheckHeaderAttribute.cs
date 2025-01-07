using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace backend.Filters
{
    public class CheckHeaderAttribute : ActionFilterAttribute
    {
        private readonly string _headerName = "x-posapp-header";
        private readonly string _expectedValue;

        public CheckHeaderAttribute(IConfiguration configuration)
        {
        // _expectedValue = configuration["ApiSettings:HeaderSecretKey"] ?? throw new ArgumentNullException("ApiSettings:HeaderSecretKey");
            _expectedValue = configuration["ApiSettings:HeaderSecretKey"];
            Console.WriteLine($"Expected Value from Config: {_expectedValue}");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            
            if (headers.ContainsKey("Cookie"))
            {
                var cookies = headers["Cookie"].ToString();
                // Console.WriteLine($"All Cookies: {cookies}");

                // แยกค่าคุกกี้ออกมาในรูปแบบ Key-Value
                    var cookiesDictionary = cookies.Split(';')
                                                .Select(c => c.Split('='))
                                                .Where(c => c.Length == 2)
                                                .ToDictionary(c => c[0].Trim(), c => c[1].Trim());
                
                if (cookiesDictionary.TryGetValue("authToken", out var authToken))
                {
                    Console.WriteLine($"AuthToken: {authToken}");
                }
                else
                {
                    Console.WriteLine("AuthToken not found in cookies.");
                }
            }
            else
            {
                Console.WriteLine("No cookies found in the request headers.");
            }

            // ตรวจสอบว่า Endpoint นี้มี [AllowAnonymous] หรือไม่
            var endpointMeta = context.HttpContext.GetEndpoint()?.Metadata;

            // ถ้ามี [AllowAnonymous] ให้ข้ามการตรวจ Header
            if (endpointMeta != null && endpointMeta.GetMetadata<IAllowAnonymous>() != null)
            {
                base.OnActionExecuting(context); // ข้ามการตรวจสอบ Header
                return;
            }

            // ตรวจสอบ Header ว่ามี "x-posapp-header" หรือไม่
            // var headers = context.HttpContext.Request.Headers;
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