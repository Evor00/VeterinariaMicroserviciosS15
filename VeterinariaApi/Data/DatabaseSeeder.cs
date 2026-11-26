using VeterinariaApi.Models;
namespace VeterinariaApi.Data;
public static class DatabaseSeeder
{
    public static async Task SeedAsync(VeterinariaContext db, IConfiguration configuration)
    {
        if (!db.Mascotas.Any())
        {
            db.Mascotas.AddRange(
                new Mascota { Nombre = "Luna", Especie = "Perro", Edad = 4, CodigoChip = "CHIP-1001" },
                new Mascota { Nombre = "Michi", Especie = "Gato", Edad = 2, CodigoChip = "CHIP-1002" },
                new Mascota { Nombre = "Kiwi", Especie = "Conejo", Edad = 1, CodigoChip = "CHIP-1003" });
            await db.SaveChangesAsync();
        }
        if (!db.Citas.Any())
        {
            db.Citas.AddRange(
                new Cita { FechaHora = new DateTime(2026, 11, 10, 9, 0, 0), Motivo = "Vacunación anual", Veterinario = "Dra. Ramos", MascotaId = 1 },
                new Cita { FechaHora = new DateTime(2026, 11, 11, 16, 30, 0), Motivo = "Control general", Veterinario = "Dr. Paredes", MascotaId = 2 });
            await db.SaveChangesAsync();
        }
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password) && !db.Usuarios.Any(x => x.Email == email))
        {
            var (hash, salt) = PasswordHasher.Hash(password);
            db.Usuarios.Add(new Usuario { Nombre = "Administrador", Email = email, Rol = "Administrador", PasswordHash = hash, PasswordSalt = salt });
            await db.SaveChangesAsync();
        }
    }
}
