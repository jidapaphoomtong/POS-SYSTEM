using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

//ตัวอย่าง
namespace backend
{
    public class CheckHeaderAndRoleAttribute : ActionFilterAttribute
    {
        private readonly string _headerName;
        private readonly string _expectedValue;
        private readonly string _requiredRole; // Role ที่ต้องการ

        public CheckHeaderAndRoleAttribute(string headerName, string expectedValue, string requiredRole)
        {
            _headerName = headerName;
            _expectedValue = expectedValue;
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            
            // ตรวจสอบ Header
            if (!headers.ContainsKey(_headerName) || headers[_headerName] != _expectedValue)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Success = false,
                    Message = $"Invalid header: {_headerName}"
                });
                return;
            }

            // ตรวจสอบ Role
            var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != _requiredRole)
            {
                context.Result = new ForbidResult($"User doesn't have required role: {_requiredRole}");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}