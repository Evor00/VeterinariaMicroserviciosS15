using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Data;
using VeterinariaApi.Models;
namespace VeterinariaApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CitasController(VeterinariaContext context) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Cita>>> GetCitas([FromQuery] DateTime? desde, [FromQuery] int? mascotaId)
    {
        IQueryable<Cita> query = context.Citas.AsNoTracking().Include(x => x.Mascota);
        if (desde.HasValue) query = query.Where(x => x.FechaHora >= desde.Value);
        if (mascotaId.HasValue) query = query.Where(x => x.MascotaId == mascotaId.Value);
        return Ok(await query.OrderBy(x => x.FechaHora).ToListAsync());
    }
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<ActionResult<Cita>> GetCita(int id) => await context.Citas.AsNoTracking().Include(x => x.Mascota).FirstOrDefaultAsync(x => x.CitaId == id) is { } cita ? Ok(cita) : NotFound();
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<ActionResult<Cita>> PostCita(Cita cita)
    {
        if (!await context.Mascotas.AnyAsync(x => x.MascotaId == cita.MascotaId)) return BadRequest("La mascota indicada no existe.");
        if (await context.Citas.AnyAsync(x => x.MascotaId == cita.MascotaId && x.FechaHora == cita.FechaHora)) return Conflict("La mascota ya tiene una cita en esa fecha y hora.");
        context.Citas.Add(cita); await context.SaveChangesAsync(); return CreatedAtAction(nameof(GetCita), new { id = cita.CitaId }, cita);
    }
    [HttpPut("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> PutCita(int id, Cita cita)
    {
        if (id != cita.CitaId) return BadRequest("El id de la ruta no coincide con el cuerpo.");
        if (!await context.Citas.AnyAsync(x => x.CitaId == id)) return NotFound();
        if (!await context.Mascotas.AnyAsync(x => x.MascotaId == cita.MascotaId)) return BadRequest("La mascota indicada no existe.");
        if (await context.Citas.AnyAsync(x => x.CitaId != id && x.MascotaId == cita.MascotaId && x.FechaHora == cita.FechaHora)) return Conflict("La mascota ya tiene una cita en esa fecha y hora.");
        context.Entry(cita).State = EntityState.Modified; await context.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteCita(int id)
    {
        var cita = await context.Citas.FindAsync(id); if (cita is null) return NotFound();
        context.Citas.Remove(cita); await context.SaveChangesAsync(); return NoContent();
    }
}
