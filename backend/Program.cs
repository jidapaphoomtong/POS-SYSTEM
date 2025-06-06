using System.Security.Claims;
using System.Text;
using backend;
using backend.Filters;
using backend.Services;
// using backend.Services.AdminService;
using backend.Services.AuthService;
using backend.Services.BranchService;
using backend.Services.CategoryService;
using backend.Services.EmployeeService;
using backend.Services.ProductService;
using backend.Services.Tokenservice;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Google.Cloud.Firestore;
using backend.Services.PurchaseService;
using backend.Services.NotificationService;


var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .Build();

builder.WebHost.UseConfiguration(configuration);
var configSection = configuration.GetSection(nameof(JwtSettings));
var settings = new JwtSettings();

configSection.Bind(settings);

builder.Services.AddSingleton<IConfiguration>(_ => configuration);

// โหลดค่าการตั้งค่า Firestore จาก appsettings.json
builder.Services.Configure<FirestoreSettings>(builder.Configuration.GetSection("FIREBASE"));

// ลงทะเบียน FirestoreDB เป็น Service ด้วย DI
builder.Services.AddSingleton<FirestoreDB>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<FirestoreSettings>>().Value;
    return new FirestoreDB(settings);
});

// เพิ่มการตั้งค่า CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://jidapa-frontend-service-qh6is2mgxa-as.a.run.app") // ระบุโดเมนที่อนุญาต
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()); // เปิดใช้งาน Cookie
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization", // Header field name สำหรับ JWT Token
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",       // ต้องใช้ "bearer" เพื่อรองรับ Authorization Header
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhb...\""
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>() // No specific scopes required
            }
        });
    });

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// builder.Services.AddScoped<IAdminService, AdminService>();
// builder.Services.AddScoped<backend.Filters.CheckHeaderAttribute>();

builder.Services.AddOptions();
builder.Services.Configure<JwtSettings>(configSection);
builder.Services.AddMvc();
builder.Services.AddAuthentication(options =>
{
    // ตั้งค่า Default Scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = settings.Issuer,
        ValidateAudience = true,
        ValidAudience = settings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // ลดเวลา Clock Skew เพื่อป้องกัน Token หมดอายุช้ากว่าเวลาจริง
        RoleClaimType = ClaimTypes.Role, // ระบุว่า Role ใช้ ClaimTypes.Role
        NameClaimType = ClaimTypes.Name,
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.ContentType = "application/json";
                var response = new { Success = false, Message = "Token has expired. Please re-authenticate." };
                var json = System.Text.Json.JsonSerializer.Serialize(response);
                return context.Response.WriteAsync(json);
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "PosAppCookie"; // ชื่อ Cookie
        options.Cookie.HttpOnly = true; // ป้องกันการเข้าถึงผ่าน JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ใช้ HTTPS เท่านั้น
        options.Cookie.SameSite = SameSiteMode.Strict; // ป้องกัน Cross-Site Attack
        options.LoginPath = "/api/Auth/login"; // Path สำหรับ Login
        options.LogoutPath = "/api/Auth/logout"; // Path สำหรับ Logout
        options.SlidingExpiration = true; // รีเฟรชอายุใช้งานของ Cookie หากใช้งานอยู่
        options.ExpireTimeSpan = TimeSpan.FromDays(3); // อายุ Cookie

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized; // ส่ง JSON Unauthorized
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Unauthorized. Please login."
                }));
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden; // ส่ง JSON Forbidden
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "Access Denied: You do not have the required permissions."
                }));
            }
        };
    });

// อ่านค่า JwtSettings จาก appsettings.json
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
// ถ้าคุณต้องการการเข้าถึง JwtSettings โดยตรง ใช้ AddSingleton
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);
// builder.Services.AddSingleton<TokenService>();

builder.Services.AddControllers(options =>
    {
        // บังคับใช้ Policy Global (ยกเว้นเฉพาะ [AllowAnonymous])
        var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
    });

builder.Services.AddSingleton<IConfiguration>(builder.Configuration); // ให้บริการ IConfiguration

// Explicitly configure URLs to listen on
builder.WebHost.UseUrls("http://*:5293");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        options.RoutePrefix = string.Empty; // Make Swagger UI accessible at root (/)
    });
}

app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin","https://jidapa-frontend-service-qh6is2mgxa-as.a.run.app");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, , x-posapp-header");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true"); // ต้องเพิ่มค่าตรงนี้
        context.Response.StatusCode = 204; // No Content
        return;
    }
    await next();
});

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // เปิดใช้งาน Controller ที่มีในโปรเจกต์
app.Run();
