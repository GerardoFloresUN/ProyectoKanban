namespace ProyectoKanban.Models
{
    public class UsuarioTareaStats
    {
        public string Usuario { get; set; }
        public int Completadas { get; set; }
        public int Pendientes { get; set; }
    }

    public class DashboardViewModel
    {
        public List<UsuarioTareaStats> TareasPorUsuario { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
