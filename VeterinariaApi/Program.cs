using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using VeterinariaApi;
using VeterinariaApi.Data;

var builder = WebApplication.CreateBuilder(args);
var jwt = JwtSettings.From(builder.Configuration);
var databaseConnection = NormalizePostgresConnectionString(
    builder.Configuration.GetConnectionString("Veterinaria")
    ?? throw new InvalidOperationException("Configure ConnectionStrings__Veterinaria."));

builder.Services.AddControllers();
builder.Services.AddDbContext<VeterinariaContext>(options =>
    options.UseNpgsql(databaseConnection));
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

static string NormalizePostgresConnectionString(string value)
{
    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
        || (uri.Scheme is not "postgres" and not "postgresql"))
    {
        return value;
    }

    var credentials = uri.UserInfo.Split(':', 2);
    if (credentials.Length != 2)
    {
        throw new InvalidOperationException("The PostgreSQL URL must include a username and password.");
    }

    var connection = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1])
    };

    foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
    {
        var values = pair.Split('=', 2);
        if (values.Length == 2 && string.Equals(values[0], "sslmode", StringComparison.OrdinalIgnoreCase)
            && Enum.TryParse<Npgsql.SslMode>(Uri.UnescapeDataString(values[1]), true, out var sslMode))
        {
            connection.SslMode = sslMode;
        }
    }

    return connection.ConnectionString;
}
