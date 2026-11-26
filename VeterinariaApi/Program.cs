using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VeterinariaApi;
using VeterinariaApi.Data;

var builder = WebApplication.CreateBuilder(args);
var jwt = JwtSettings.From(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddDbContext<VeterinariaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Veterinaria")
        ?? throw new InvalidOperationException("Configure ConnectionStrings__Veterinaria.")));
builder.Services.AddSingleton(jwt);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VeterinariaContext>();
    await db.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(db, builder.Configuration);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "VeterinariaApi" })).AllowAnonymous();
app.MapControllers();
app.Run();
