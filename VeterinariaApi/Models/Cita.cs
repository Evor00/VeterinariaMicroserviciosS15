using System.ComponentModel.DataAnnotations;
namespace VeterinariaApi.Models;
public class Cita
{
    public int CitaId { get; set; }
    [Required] public DateTime FechaHora { get; set; }
    [Required, StringLength(150)] public string Motivo { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Veterinario { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }
}
