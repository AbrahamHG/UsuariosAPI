using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Modelos;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuarioController : ControllerBase
{
    private readonly ApiDbContext _db;
    private readonly PasswordHasher<Usuario> _hasher;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(ApiDbContext db, ILogger<UsuarioController> logger)
    {
        _db = db;
        _hasher = new PasswordHasher<Usuario>();
        _logger = logger;
    }

    private string GetClientIp()
    {
        //Configurar para poner real para auditoria
        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip == null) return "IP desconocida";

        if (IPAddress.IsLoopback(ip))
            return "127.0.0.1";

        return ip.ToString(); ;
    }

    // GET: api/usuario
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Inicio consulta de usuarios por {User} desde IP {IP}",
            User.Identity?.Name ?? "Anonimo", GetClientIp());

        try
        {
            var usuarios = await _db.Usuarios.ToListAsync();

            var dtoList = usuarios.Select(u => new Usuario_DTO
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol,
                FechaCreacion = u.FechaCreacion
            });

            _logger.LogInformation("Consulta de usuarios completada. Total: {Count}", dtoList.Count());

            return Ok(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios por {User} desde IP {IP}",
                User.Identity?.Name ?? "Anonimo", GetClientIp());

            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }

    // GET: api/usuario/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Inicio consulta de usuario {Id} por {User} desde IP {IP}",
            id, User.Identity?.Name ?? "Anonimo", GetClientIp());

        try
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario {Id} no encontrado", id);
                return NotFound(new { mensaje = "Usuario no encontrado" });
            }

            var dto = new Usuario_DTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                FechaCreacion = usuario.FechaCreacion
            };

            _logger.LogInformation("Usuario {Id} consultado correctamente", id);

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar usuario {Id} por {User} desde IP {IP}",
                id, User.Identity?.Name ?? "Anonimo", GetClientIp());

            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }

    // POST: api/usuario
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Register_DTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            _logger.LogWarning("Error de validación en CrearUsuario por {User} desde IP {IP}: {Errores}",
                User.Identity?.Name ?? "Anonimo",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                string.Join(" | ", errores));

            return BadRequest(new { errors = errores });
        }

        _logger.LogInformation("Inicio creación de usuario {Email} por {User} desde IP {IP}",
            dto?.Email, User.Identity?.Name ?? "Anonimo", GetClientIp());

        try
        {
            if (dto == null || string.IsNullOrEmpty(dto.Nombre) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest(new { mensaje = "Datos inválidos" });

            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { mensaje = "El correo ya está registrado" });

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Rol = dto.Rol,
                FechaCreacion = DateTime.UtcNow,
                Password = _hasher.HashPassword(new Usuario(), dto.Password)
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            // Auditoría
            var log = new Log
            {
                UsuarioId = usuario.Id,
                Accion = "Insertar",
                Fecha = DateTime.UtcNow,
                RealizadoPor = GetClientIp(),
                Cambios = $"Usuario creado: {usuario.Nombre}"
                
            };
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Usuario {Nombre} creado exitosamente", usuario.Nombre);

            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, new Usuario_DTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                FechaCreacion = usuario.FechaCreacion
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario {Email}", dto?.Email);

            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }

    // PUT: api/usuario/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdate_DTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            _logger.LogWarning("Error de validación en ActualizarUsuario {Id} por {User}: {Errores}",
                id,
                User.Identity?.Name ?? "Anonimo",
                string.Join(" | ", errores));

            return BadRequest(new { errors = errores });
        }
        _logger.LogInformation("Inicio actualización de usuario {Id} por {User}", id, User.Identity?.Name ?? "Anonimo");

        try
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            if (dto == null || string.IsNullOrEmpty(dto.Nombre) || string.IsNullOrEmpty(dto.Rol))
                return BadRequest(new { mensaje = "Datos inválidos" });

            usuario.Nombre = dto.Nombre;
            usuario.Rol = dto.Rol;
            usuario.FechaActualizacion = DateTime.UtcNow;

            _db.Usuarios.Update(usuario);
            await _db.SaveChangesAsync();

            var log = new Log
            {
                UsuarioId = usuario.Id,
                Accion = "Actualizar",
                Fecha = DateTime.UtcNow,
                RealizadoPor = GetClientIp(),
                Cambios = $"Usuario actualizado: {usuario.Nombre}"
            };
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Usuario {Id} actualizado correctamente", id);

            return Ok(new Usuario_DTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                FechaCreacion = usuario.FechaCreacion
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {Id}", id);
            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }

    // DELETE: api/usuario/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Inicio eliminación de usuario {Id} por {User}", id, User.Identity?.Name ?? "Anonimo");

        try
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var nombreUsuario = usuario.Nombre;

            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync();


            var log = new Log
            {
                UsuarioId = null,
                Accion = "Eliminar",
                Fecha = DateTime.UtcNow,
                RealizadoPor = GetClientIp(),
                Cambios = $"Usuario borrado:{id},Nombre:{nombreUsuario}"
            };

            _db.Logs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Usuario {Id} eliminado correctamente", id);

            return Ok(new { mensaje = "Usuario eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }
}