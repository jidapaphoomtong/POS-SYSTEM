// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// //ตัวอย่าง
// namespace backend
// {
//     public class CheckHeader : Attribute, IActionFilter
//     {
//         private readonly string _headerName;
//         private readonly string _expectedValue;

//         public CheckHeaderAttribute(string headerName, string expectedValue)
//         {
//             _headerName = headerName;
//             _expectedValue = expectedValue;
//         }

//         // This method runs before executing the action
//         public void OnActionExecuting(ActionExecutingContext context)
//         {
//             if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var headerValue) 
//                 || headerValue != _expectedValue)
//             {
//                 context.Result = new BadRequestObjectResult($"The required header '{_headerName}' is missing or invalid.");
//             }
//         }

//         // This method is for after action execution (not used here)
//         public void OnActionExecuted(ActionExecutedContext context) { }
//     }
//         {
            
//         }
// }