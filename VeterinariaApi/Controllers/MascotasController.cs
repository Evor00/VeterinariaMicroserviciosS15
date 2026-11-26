using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Data;
using VeterinariaApi.Models;
namespace VeterinariaApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class MascotasController(VeterinariaContext context) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Mascota>>> GetMascotas([FromQuery] string? especie, [FromQuery] string? nombre)
    {
        IQueryable<Mascota> query = context.Mascotas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(especie)) query = query.Where(x => x.Especie == especie);
        if (!string.IsNullOrWhiteSpace(nombre)) query = query.Where(x => x.Nombre.Contains(nombre));
        return Ok(await query.OrderBy(x => x.Nombre).ToListAsync());
    }
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<ActionResult<Mascota>> GetMascota(int id) => await context.Mascotas.AsNoTracking().FirstOrDefaultAsync(x => x.MascotaId == id) is { } mascota ? Ok(mascota) : NotFound();
    [HttpGet("{id:int}/citas"), AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Cita>>> GetCitasDeMascota(int id)
    {
        if (!await context.Mascotas.AnyAsync(x => x.MascotaId == id)) return NotFound();
        return Ok(await context.Citas.AsNoTracking().Where(x => x.MascotaId == id).OrderBy(x => x.FechaHora).ToListAsync());
    }
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<ActionResult<Mascota>> PostMascota(Mascota mascota)
    {
        if (await context.Mascotas.AnyAsync(x => x.CodigoChip == mascota.CodigoChip)) return Conflict("El código de chip ya está registrado.");
        context.Mascotas.Add(mascota); await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMascota), new { id = mascota.MascotaId }, mascota);
    }
    [HttpPut("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> PutMascota(int id, Mascota mascota)
    {
        if (id != mascota.MascotaId) return BadRequest("El id de la ruta no coincide con el cuerpo.");
        if (!await context.Mascotas.AnyAsync(x => x.MascotaId == id)) return NotFound();
        if (await context.Mascotas.AnyAsync(x => x.MascotaId != id && x.CodigoChip == mascota.CodigoChip)) return Conflict("El código de chip ya está registrado.");
        context.Entry(mascota).State = EntityState.Modified; await context.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteMascota(int id)
    {
        var mascota = await context.Mascotas.FindAsync(id); if (mascota is null) return NotFound();
        if (await context.Citas.AnyAsync(x => x.MascotaId == id)) return Conflict("No se puede eliminar una mascota que tiene citas registradas.");
        context.Mascotas.Remove(mascota); await context.SaveChangesAsync(); return NoContent();
    }
}
