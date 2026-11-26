using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Models;
namespace VeterinariaApi.Data;
public class VeterinariaContext(DbContextOptions<VeterinariaContext> options) : DbContext(options)
{
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mascota>(entity => { entity.ToTable("Mascota"); entity.HasIndex(x => x.CodigoChip).IsUnique(); });
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.ToTable("Cita"); entity.Property(x => x.FechaHora).HasColumnType("timestamp without time zone");
            entity.HasIndex(x => new { x.MascotaId, x.FechaHora }).IsUnique();
            entity.HasOne(x => x.Mascota).WithMany(x => x.Citas).HasForeignKey(x => x.MascotaId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Usuario>(entity => { entity.ToTable("Usuario"); entity.HasIndex(x => x.Email).IsUnique(); });
    }
}
