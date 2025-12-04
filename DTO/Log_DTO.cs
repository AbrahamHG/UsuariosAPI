using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Log_DTO
    {
        public int Id { get; set; }
        public string Accion { get; set; }
        public DateTime Fecha { get; set; }
        public string RealizadoPor { get; set; }
        public string Cambios { get; set; }

        // Datos del usuario relacionado
        public string UsuarioNombre { get; set; }
        public string UsuarioEmail { get; set; }
    }
}
