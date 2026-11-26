namespace GatewayApi;
public sealed record JwtSettings(string Key, string Issuer, string Audience)
{
    public static JwtSettings From(IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32) throw new InvalidOperationException("Configure Jwt__Key with at least 32 characters.");
        return new JwtSettings(key, configuration["Jwt:Issuer"] ?? "VeterinariaMicroservicios", configuration["Jwt:Audience"] ?? "VeterinariaClientes");
    }
}
