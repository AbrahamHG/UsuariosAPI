using System;
using System.Collections.Generic;

namespace UsuariosAPI.Modelos;

public partial class Log
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public string Accion { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public string? RealizadoPor { get; set; }

    public string? Cambios { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
