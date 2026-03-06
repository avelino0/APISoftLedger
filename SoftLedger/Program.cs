using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var key = builder.Configuration["JwtKey"];
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)
        )
    };
});

var app = builder.Build();
app.Run($"http://0.0.0.0:{port}");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();//