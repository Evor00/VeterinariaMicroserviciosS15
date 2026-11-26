# Microservicios con API Gateway y Ocelot Semana 15

Solución de clínica veterinaria formada por tres aplicaciones ASP.NET Core .NET 10. El cliente consume el gateway en `http://localhost:5057`.

| Proyecto | Puerto | Responsabilidad |
|---|---:|---|
| `VeterinariaApi` | 5055 | API principal con PostgreSQL, JWT, mascotas y citas. |
| `ClientesApi` | 5056 | Clientes en memoria, protegidos por JWT. |
| `GatewayApi` | 5057 | Único punto de entrada con Ocelot 25.x. |

## Seguridad

Los tres proyectos usan `Jwt__Key`, `Jwt__Issuer` y `Jwt__Audience`. Nunca subas una clave JWT o cadena de conexión. Para desarrollo, define variables seguras antes de ejecutar los servicios:

```powershell
$env:Jwt__Key = "<clave-local-de-al-menos-32-caracteres>"
$env:Jwt__Issuer = "VeterinariaMicroservicios"
$env:Jwt__Audience = "VeterinariaClientes"
$env:SeedAdmin__Email = "<correo-administrador-local>"
$env:SeedAdmin__Password = "<contraseña-administrador-local>"
```

La cuenta administradora local se crea únicamente en desarrollo. En Render configura las variables en el entorno y ejecuta `VeterinariaApi/database/Veterinaria.sql` antes de probar la API.

## Ejecución local

Con PostgreSQL en el puerto 5432 y la base `veterinaria_s15`, abre tres terminales:

```powershell
dotnet run --project VeterinariaApi
dotnet run --project ClientesApi
dotnet run --project GatewayApi
```

Rutas: `POST /gateway/auth/register`, `POST /gateway/auth/login`, `GET /gateway/veterinaria/mascotas`, escritura `POST|PUT|DELETE /gateway/veterinaria/...` y `GET /gateway/clientes/{everything}`. La escritura exige el rol `Administrador`; clientes exige JWT válido.

## Render

`render.yaml` declara tres servicios web, PostgreSQL y un grupo de entorno con un único secreto JWT compartido. El gateway toma los hostnames internos por `fromService`, sin depender de localhost.
