using System;
using System.Collections.Generic;

namespace UsuariosAPI.Modelos;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public string Password { get; set; } = null!;

}
