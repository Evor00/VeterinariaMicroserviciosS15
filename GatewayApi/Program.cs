using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using GatewayApi;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
ApplyServiceEndpoint(builder.Configuration, "Veterinaria", 0, 1, 2);
ApplyServiceEndpoint(builder.Configuration, "Clientes", 3);

var jwt = JwtSettings.From(builder.Configuration);
builder.Services.AddSingleton(jwt);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
    });
builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        await context.Response.WriteAsJsonAsync(new { status = "ok", service = "GatewayApi" });
        return;
    }
    await next();
});
await app.UseOcelot();
app.Run();

static void ApplyServiceEndpoint(IConfiguration configuration, string serviceName, params int[] routeIndexes)
{
    var url = configuration[$"ServiceEndpoints:{serviceName}Url"];
    if (Uri.TryCreate(url, UriKind.Absolute, out var endpoint))
    {
        foreach (var index in routeIndexes)
        {
            configuration[$"Routes:{index}:DownstreamHostAndPorts:0:Host"] = endpoint.Host;
            configuration[$"Routes:{index}:DownstreamHostAndPorts:0:Port"] = endpoint.IsDefaultPort
                ? (endpoint.Scheme == Uri.UriSchemeHttps ? "443" : "80")
                : endpoint.Port.ToString();
            configuration[$"Routes:{index}:DownstreamScheme"] = endpoint.Scheme;
        }
        return;
    }

    var value = configuration[$"ServiceEndpoints:{serviceName}Host"];
    if (string.IsNullOrWhiteSpace(value)) return;
    var split = value.LastIndexOf(':');
    var host = split > 0 ? value[..split] : value;
    var port = split > 0 && int.TryParse(value[(split + 1)..], out var parsed) ? parsed : 80;
    foreach (var index in routeIndexes)
    {
        configuration[$"Routes:{index}:DownstreamHostAndPorts:0:Host"] = host;
        configuration[$"Routes:{index}:DownstreamHostAndPorts:0:Port"] = port.ToString();
        configuration[$"Routes:{index}:DownstreamScheme"] = "http";
    }
}
