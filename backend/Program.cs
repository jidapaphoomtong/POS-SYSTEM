using backend.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthorization();

app.MapControllers(); // เปิดใช้งาน Controller ที่มีในโปรเจกต์

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// // Firestore endpoint: ดึงข้อมูลจาก Firestore
// app.MapGet("/document/request", async (FirestoreDB db) =>
// {
//     var data = await db.GetCollectionAsync("etax_bill");
//     return data;
// }).WithName("GetFirestoreDB");


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}