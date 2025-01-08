using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Security.Claims;

public class LogActionAttribute : ActionFilterAttribute
{
    // เรียกก่อน Action ทำงาน
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        //  // ดึง User Claims
        // var userName = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
        // var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "No Role Assigned";

        var controllerName = context.RouteData.Values["controller"];
        var actionName = context.RouteData.Values["action"];

        // Log รายละเอียดข้อมูล
        Console.WriteLine($"[OnActionExecuting] Controller: {controllerName}, Action: {actionName}");
        
        if (!context.HttpContext.User.Claims.Any())
        {
            Console.WriteLine("No claims found in HttpContext.User.");
        }
        else
        {
            foreach (var claim in context.HttpContext.User.Claims)
            {
                // Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }
        }

        base.OnActionExecuting(context);
    }

    // เรียกหลังจาก Action ทำงานเสร็จ
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var controllerName = context.RouteData.Values["controller"];
        var actionName = context.RouteData.Values["action"];
        Console.WriteLine($"[OnActionExecuted] Controller: {controllerName}, Action: {actionName}");
        base.OnActionExecuted(context);
    }

    // เรียกก่อน Result (View) ถูก Render
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"];
        var actionName = context.RouteData.Values["action"];
        Console.WriteLine($"[OnResultExecuting] Controller: {controllerName}, Action: {actionName}");
        base.OnResultExecuting(context);
    }

    // เรียกหลังจาก Result (View) ถูก Render แล้ว
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        var controllerName = context.RouteData.Values["controller"];
        var actionName = context.RouteData.Values["action"];
        Console.WriteLine($"[OnResultExecuted] Controller: {controllerName}, Action: {actionName}");
        base.OnResultExecuted(context);
    }

    // ฟังก์ชันสำหรับ Log Message
    private void Log(string methodName, FilterContext context)
    {
        var routeData = context.RouteData;
        var controllerName = routeData.Values["controller"];
        var actionName = routeData.Values["action"];
        var logMessage = $"{methodName} - Controller: {controllerName}, Action: {actionName}";
        Debug.WriteLine(logMessage, "ActionFilterLog");

        // คุณสามารถบันทึก Log ลง Database หรือส่งข้อความไปยัง External Logging Service ได้เช่นกัน
    }
}