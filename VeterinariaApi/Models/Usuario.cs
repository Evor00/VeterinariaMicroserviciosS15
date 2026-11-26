using System.ComponentModel.DataAnnotations;
namespace VeterinariaApi.Models;
public class Usuario
{
    public int UsuarioId { get; set; }
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(120), EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Rol { get; set; } = "Usuario";
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [Required] public string PasswordSalt { get; set; } = string.Empty;
}
