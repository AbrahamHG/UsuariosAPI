using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UsuariosAPI.Modelos;
using DTO;
using Microsoft.EntityFrameworkCore;
using System.Net;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ApiDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<Usuario> _hasher;
    private readonly ILogger<LoginController> _logger;

    public LoginController(ApiDbContext db, IConfiguration config, ILogger<LoginController> logger)
    {
        _db = db;
        _config = config;
        _hasher = new PasswordHasher<Usuario>();
        _logger = logger;
    }

    private string GetClientIp()
    {   //Configurar para poner real para auditoria
        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip == null) return "IP desconocida";

        if (IPAddress.IsLoopback(ip))
            return "127.0.0.1";

        return ip.ToString(); ;
    }

    // POST: api/login/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register_DTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // Registrar en el logger
            _logger.LogWarning("Error de validación en Register por {User} desde IP {IP}: {Errores}",
                User.Identity?.Name ?? "Anonimo",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                string.Join(" | ", errores));


            return BadRequest(new { errors = errores });
        }


        _logger.LogInformation("Inicio registro de usuario {Email} desde IP {IP}", dto.Email, GetClientIp());

        try
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogWarning("Intento de registro con correo ya existente: {Email}", dto.Email);
                return BadRequest(new { mensaje = "El correo ya está registrado" });
            }

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
                Accion = "Registro",
                Fecha = DateTime.UtcNow,
                RealizadoPor = GetClientIp(),
                Cambios = $"Usuario registrado: {usuario.Email}"
            };
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Usuario {Email} registrado correctamente", usuario.Email);

            return Ok(new { mensaje = "Usuario registrado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario {Email}", dto.Email);
            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }

    // POST: api/login
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] Login_DTO dto)
    {
        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            _logger.LogWarning("Error de validacion en Login por {User} desde IP {IP}: {Errores}",
                User.Identity?.Name ?? "Anonimo",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                string.Join(" | ", errores));

            return BadRequest(new { errors = errores });
        }

        _logger.LogInformation("Inicio login de usuario {Email} desde IP {IP}", dto.Email, GetClientIp());

        try
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (usuario == null)
            {
                _logger.LogWarning("Login fallido: usuario {Email} no encontrado", dto.Email);
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            var result = _hasher.VerifyHashedPassword(usuario, usuario.Password, dto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login fallido: contraseña incorrecta para {Email}", dto.Email);
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            // Claims para el token
            var claims = new[] {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            // Auditoría
            var log = new Log
            {
                UsuarioId = usuario.Id,
                Accion = "Login",
                Fecha = DateTime.UtcNow,
                RealizadoPor = GetClientIp(),
                Cambios = $"Usuario inició sesión: {usuario.Email}"
            };
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Usuario {Email} inició sesión correctamente", usuario.Email);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión usuario {Email}", dto.Email);
            return StatusCode(500, new { mensaje = "Ocurrió un error interno, contacte al administrador." });
        }
    }
}