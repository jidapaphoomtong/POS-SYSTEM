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

            if (!headers.ContainsKey(_headerName) || headers[_headerName] != _expectedValue)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Success = false,
                    Message = $"Invalid header: {_headerName}"
                });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}