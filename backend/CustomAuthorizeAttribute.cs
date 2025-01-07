using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

public class CustomAuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public CustomAuthorizeRoleAttribute(string roles)
    {
        _roles = roles.Split(',').Select(role => role.Trim()).ToArray();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user == null || !user.Identity.IsAuthenticated)
        {
            // ส่ง 401 Unauthorized ถ้าผู้ใช้ยังไม่ล็อกอิน
            context.Result = new UnauthorizedResult();
            return;
        }

        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        if (!_roles.Any(requiredRole => userRoles.Contains(requiredRole)))
        {
            // ส่ง 403 Forbidden ถ้าผู้ใช้ไม่มีสิทธิ์
            context.Result = new ForbidResult();
        }
    }
}