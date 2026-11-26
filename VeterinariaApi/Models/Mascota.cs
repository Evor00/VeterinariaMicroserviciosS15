using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace VeterinariaApi.Models;
public class Mascota
{
    public int MascotaId { get; set; }
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(40)] public string Especie { get; set; } = string.Empty;
    [Range(0, 40)] public int Edad { get; set; }
    [Required, StringLength(25)] public string CodigoChip { get; set; } = string.Empty;
    [JsonIgnore] public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
