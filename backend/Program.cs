using System.Text;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// **ตั้งค่า JWT Authentication**
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // ตรวจสอบว่าผู้สร้าง Token คือใคร
            ValidateAudience = true, // ตรวจสอบผู้ใช้งาน Audience
            ValidateLifetime = true, // ตรวจสอบว่าหมดอายุหรือยัง
            ValidateIssuerSigningKey = true, // ตรวจสอบ Signature
            ValidIssuer = "localhost", // Issuer ที่เราเชื่อถือ
            ValidAudience = "localhost", // Audience ที่เราเชื่อถือ
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("wrZzjoEgLiypg53ojlxG")) // Secret Key
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin() // อนุญาตทุก Origin
                          .AllowAnyMethod() // อนุญาตทุก HTTP Method
                          .AllowAnyHeader() // อนุญาตทุก Header
    );
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Explicitly configure URLs to listen on
builder.WebHost.UseUrls("http://*:5293");

var app = builder.Build();

app.UseCors("AllowAllOrigins");

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
