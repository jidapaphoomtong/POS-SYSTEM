using System.Text;
using backend.Services;
using backend.Services.AdminService;
using backend.Services.AuthService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// **ตั้งค่า JWT Authentication**
var SecretKey = builder.Configuration["JwtSettings:SecretKey"];

if (string.IsNullOrEmpty(SecretKey))
{
    throw new NullReferenceException("SecretKey is missing.");
}

// ถ้า Key สั้นกว่า 32 ตัวอักษร ให้เติมจนยาวเพียงพอ
if (SecretKey.Length < 32)
{
    SecretKey = SecretKey.PadRight(32, 'X'); // เติม 'X' ให้ครบ 32 ตัว
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/",
            ValidAudience = "https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
        };
    });

// โหลดค่าการตั้งค่า Firestore จาก appsettings.json
builder.Services.Configure<FirestoreSettings>(builder.Configuration.GetSection("FirestoreSettings"));

// ลงทะเบียน FirestoreDB เป็น Service ด้วย DI
builder.Services.AddSingleton<FirestoreDB>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<FirestoreSettings>>().Value;
    return new FirestoreDB(settings);
});

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAllOrigins",
//         builder => builder.AllowAnyOrigin() // อนุญาตทุก Origin
//                           .AllowAnyMethod() // อนุญาตทุก HTTP Method
//                           .AllowAnyHeader() // อนุญาตทุก Header
//     );
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // URL จาก React
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhbG...\""
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add services to the DI container.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddControllers(options =>
{
    // บังคับใช้ Policy Global (ยกเว้นเฉพาะ [AllowAnonymous])
    var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

builder.Services.AddAuthorization(options =>
{
    // กำหนด Policy สำหรับ Role ต่าง ๆ
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("admin"));
    options.AddPolicy("ManagerPolicy", policy => policy.RequireRole("manager"));
    options.AddPolicy("EmployeePolicy", policy => policy.RequireRole("employee"));
});


// Explicitly configure URLs to listen on
builder.WebHost.UseUrls("https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/");

var app = builder.Build();

// app.UseCors("AllowAllOrigins");
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // เปิดใช้งาน Controller ที่มีในโปรเจกต์
app.Run();
