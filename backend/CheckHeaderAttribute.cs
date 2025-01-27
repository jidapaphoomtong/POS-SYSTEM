using backend.Services.Tokenservice;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace backend.Filters
{
    public class CheckHeaderAttribute : ActionFilterAttribute
    {
        private readonly string _headerName = "x-posapp-header";
        private readonly string _expectedValue;
        private readonly ITokenService _tokenService;
        private readonly ILogger<CheckHeaderAttribute> _logger;

        public CheckHeaderAttribute(IConfiguration configuration, ITokenService tokenService, ILogger<CheckHeaderAttribute> logger)
        {
            _expectedValue = configuration["ApiSettings:HeaderSecretKey"] ?? throw new ArgumentNullException("ApiSettings:HeaderSecretKey");
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            var cookies = headers["Cookie"].ToString();
            ClaimsPrincipal principal = null;

            if (!string.IsNullOrEmpty(cookies))
            {
                try
                {
                    var cookiesDictionary = cookies.Split(';')
                                                    .Select(c => c.Split('='))
                                                    .Where(c => c.Length == 2)
                                                    .ToDictionary(
                                                        c => Uri.UnescapeDataString(c[0].Trim()),
                                                        c => Uri.UnescapeDataString(c[1].Trim()));

                    if (cookiesDictionary.TryGetValue("authToken", out var authToken))
                    {
                        _logger.LogInformation($"AuthToken found: {authToken}");
                        principal = _tokenService.ValidateToken(authToken);
                        
                        if (principal == null)
                        {
                            _logger.LogWarning("Invalid or null token.");
                            // Sign out
                            _ = context.HttpContext.SignOutAsync();
                            context.Result = new UnauthorizedObjectResult(new
                            {
                                Success = false,
                                Message = "Invalid token. You have been logged out."
                            });
                            return;
                        }

                    //     // Log claims from the principal if available
                    //     foreach (var claim in principal.Claims)
                    //     {
                    //         _logger.LogInformation($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                    //     }
                    }
                    else
                    {
                        _logger.LogWarning("AuthToken not found in cookies.");
                    }
                }
                catch (SecurityTokenExpiredException)
                {
                    _logger.LogWarning("AuthToken has expired, signing out.");
                    // Sign out
                    _ = context.HttpContext.SignOutAsync();
                    context.Result = new UnauthorizedObjectResult(new
                    {
                        Success = false,
                        Message = "AuthToken has expired. You have been logged out."
                    });
                    return;
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogWarning($"Invalid AuthToken: {ex.Message}");
                    // Sign out
                    _ = context.HttpContext.SignOutAsync();
                    context.Result = new UnauthorizedObjectResult(new
                    {
                        Success = false,
                        Message = "Invalid token. You have been logged out."
                    });
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error validating AuthToken: {ex.Message}");
                    // Sign out
                    _ = context.HttpContext.SignOutAsync();
                    context.Result = new UnauthorizedObjectResult(new
                    {
                        Success = false,
                        Message = "An unexpected error occurred. You have been logged out."
                    });
                    return;
                }
            }
            else
            {
                _logger.LogWarning("No cookies found in the request headers.");
            }

            var endpointMeta = context.HttpContext.GetEndpoint()?.Metadata;

            // Skip header check if [AllowAnonymous] is present
            if (endpointMeta != null && endpointMeta.GetMetadata<IAllowAnonymous>() != null)
            {
                base.OnActionExecuting(context);
                return;
            }

            // Check required header
            // if (!headers.ContainsKey(_headerName) || headers[_headerName] != _expectedValue)
            // {
            //     _logger.LogWarning("Missing or invalid required header.");
            //     // Sign out
            //     _ = context.HttpContext.SignOutAsync(); // Clear Authentication

            //     // Return 401 Unauthorized
            //     context.Result = new UnauthorizedObjectResult(new
            //     {
            //         Success = false,
            //         Message = "Invalid or missing required header. You have been logged out."
            //     });

            //     return;
            // }

            // Set the user claims from the validated token (if valid)
            if (principal != null)
            {
                context.HttpContext.User = principal;
            }
            else
            {
                _logger.LogWarning("Token validation failed, setting empty User.");
            }

            base.OnActionExecuting(context);
        }
    }

}