using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Modelos;
using Microsoft.AspNetCore.Authorization;
using DTO;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LogsController : ControllerBase
{
    private readonly ApiDbContext _db;
    private readonly ILogger<LogsController> _logger;

    public LogsController(ApiDbContext db, ILogger<LogsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/logs
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Consulta de todos los logs por {User} desde IP {IP}",
            User.Identity?.Name ?? "Anonimo",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        try
        {
            var logs = await _db.Logs
                .OrderByDescending(l => l.Fecha)
                .Select(l => new Log_DTO
                {
                    Id = l.Id,
                    Accion = l.Accion,
                    Fecha = l.Fecha,
                    RealizadoPor = l.RealizadoPor,
                    Cambios = l.Cambios
                })
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar todos los logs por {User}", User.Identity?.Name ?? "Anonimo");
            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }

    // GET: api/logs/usuario/5
    [HttpGet("usuario/{usuarioId}")]
    public async Task<IActionResult> GetByUsuario(int usuarioId)
    {
        _logger.LogInformation("Consulta de logs para usuario {UsuarioId} por {User}",
            usuarioId, User.Identity?.Name ?? "Anonimo");

        try
        {
            var logs = await _db.Logs
                .Where(l => l.UsuarioId == usuarioId)
                .OrderByDescending(l => l.Fecha)
                .Select(l => new Log_DTO
                {
                    Id = l.Id,
                    Accion = l.Accion,
                    Fecha = l.Fecha,
                    RealizadoPor = l.RealizadoPor,
                    Cambios = l.Cambios
                })
                .ToListAsync();

            if (!logs.Any())
            {
                _logger.LogWarning("No se encontraron logs para usuario {UsuarioId}", usuarioId);
                return NotFound(new { mensaje = "No se encontraron logs para este usuario" });
            }

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar logs para usuario {UsuarioId}", usuarioId);
            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }

    // GET: api/logs/accion/Insertar
    [HttpGet("accion/{accion}")]
    public async Task<IActionResult> GetByAccion(string accion)
    {
        _logger.LogInformation("Consulta de logs por acción {Accion} realizada por {User}",
            accion, User.Identity?.Name ?? "Anonimo");

        try
        {
            var logs = await _db.Logs
                .Where(l => l.Accion == accion)
                .OrderByDescending(l => l.Fecha)
                .Select(l => new Log_DTO
                {
                    Id = l.Id,
                    Accion = l.Accion,
                    Fecha = l.Fecha,
                    RealizadoPor = l.RealizadoPor,
                    Cambios = l.Cambios
                })
                .ToListAsync();

            if (!logs.Any())
            {
                _logger.LogWarning("No se encontraron logs para acción {Accion}", accion);
                return NotFound(new { mensaje = "No se encontraron logs para esta acción" });
            }

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar logs por acción {Accion}", accion);
            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }
}