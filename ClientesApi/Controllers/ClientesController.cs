using ClientesApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ClientesApi.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ClientesController(ClientesStore store) : ControllerBase
{
    [HttpGet] public ActionResult<IReadOnlyList<Cliente>> GetClientes() => Ok(store.All);
    [HttpGet("{id:int}")] public ActionResult<Cliente> GetCliente(int id) => store.ById(id) is { } cliente ? Ok(cliente) : NotFound();
}
