// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Configuration;
// using System;
// using System.Threading.Tasks;

// namespace backend.Middlewares
// {
//     public class CheckHeaderMiddleware
//     {
//         private readonly RequestDelegate _next;
//         private readonly string _headerName = "x-posapp-header"; // ชื่อ Header ที่ต้องตรวจสอบ
//         private readonly string _expectedValue; // ค่าที่คาดหวังจากแอป

//         public CheckHeaderMiddleware(RequestDelegate next, IConfiguration configuration)
//         {
//             _next = next ?? throw new ArgumentNullException(nameof(next));
//             _expectedValue = configuration["ApiSettings:HeaderSecretKey"] 
//                             ?? throw new ArgumentNullException("ApiSettings:HeaderSecretKey");
//         }

//         public async Task Invoke(HttpContext context)
//         {
//             // ตรวจสอบว่ามี Header ตามที่กำหนดมาหรือไม่
//             if (!context.Request.Headers.TryGetValue(_headerName, out var actualValue) || actualValue != _expectedValue)
//             {
//                 // หาก Header ไม่ถูกต้อง ส่งข้อผิดพลาดกลับไปยัง Client
//                 context.Response.StatusCode = StatusCodes.Status400BadRequest; // 400: Bad Request
//                 await context.Response.WriteAsJsonAsync(new
//                 {
//                     Success = false,
//                     Message = $"Invalid header: {_headerName}" // แจ้งปัญหาในข้อความส่วนนี้
//                 });
//                 return;
//             }

//             // หาก Header ถูกต้อง เรียก Middleware ถัดไป
//             await _next(context);
//         }
//     }
// }