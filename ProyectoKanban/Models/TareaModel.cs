using System;

namespace ProyectoKanban.Models
{
    public class TareaModel
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string Estado { get; set; }

        public string? UsuarioId { get; set; }

        public string? UsuarioNombre { get; set; }

        public int Orden { get; set; }

        public int DiasAntesAlerta { get; set; }

        public bool AlertaEnviada { get; set; }
    }
}
