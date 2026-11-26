using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VeterinariaApi.Data;
using VeterinariaApi.Models;
namespace VeterinariaApi.Controllers;
public record RegisterRequest(string Nombre, string Email, string Password);
public record LoginRequest(string Email, string Password);
[Route("api/auth")]
[ApiController]
[AllowAnonymous]
public class AuthController(VeterinariaContext context, JwtSettings jwt) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(email) || request.Password.Length < 8) return BadRequest("Nombre, email y contraseña de al menos 8 caracteres son obligatorios.");
        if (await context.Usuarios.AnyAsync(x => x.Email == email)) return Conflict("El correo ya está registrado.");
        var (hash, salt) = PasswordHasher.Hash(request.Password);
        var usuario = new Usuario { Nombre = request.Nombre.Trim(), Email = email, Rol = "Usuario", PasswordHash = hash, PasswordSalt = salt };
        context.Usuarios.Add(usuario); await context.SaveChangesAsync();
        return Created($"api/auth/users/{usuario.UsuarioId}", new { usuario.UsuarioId, usuario.Nombre, usuario.Email, usuario.Rol });
    }
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var usuario = await context.Usuarios.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant());
        if (usuario is null || !PasswordHasher.Verify(request.Password, usuario.PasswordHash, usuario.PasswordSalt)) return Unauthorized();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()), new Claim(ClaimTypes.Name, usuario.Nombre), new Claim(ClaimTypes.Email, usuario.Email), new Claim(ClaimTypes.Role, usuario.Rol) };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: credentials);
        return Ok(new { accessToken = new JwtSecurityTokenHandler().WriteToken(token), tokenType = "Bearer", expiresIn = 7200, role = usuario.Rol });
    }
}
