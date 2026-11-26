namespace ClientesApi.Models;
public record Cliente(int ClienteId, string Nombre, string Documento, string Email, string Telefono);
public sealed class ClientesStore
{
    private readonly IReadOnlyList<Cliente> clientes = new List<Cliente>
    {
        new(1, "Ana Torres", "DNI 74001234", "ana.torres@example.com", "999111222"),
        new(2, "Luis Pérez", "DNI 74005678", "luis.perez@example.com", "999333444"),
        new(3, "María Ramos", "DNI 74009876", "maria.ramos@example.com", "999555666")
    };
    public IReadOnlyList<Cliente> All => clientes;
    public Cliente? ById(int id) => clientes.FirstOrDefault(x => x.ClienteId == id);
}
