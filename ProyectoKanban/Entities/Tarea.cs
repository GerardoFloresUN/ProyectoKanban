using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;


namespace ProyectoKanban.Entities
{
    public class Tarea
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string Estado { get; set; }

        public string? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public IdentityUser? Usuario { get; set; }

        public int Orden { get; set; }
    }
}
