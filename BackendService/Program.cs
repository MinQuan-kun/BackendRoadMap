using System.Text;
using BackendService.Configurations;
using BackendService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Ánh xạ cấu hình
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// Đăng ký Service
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

// Map các Endpoint
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();
