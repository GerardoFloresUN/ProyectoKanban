using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoKanban.Models;
using ProyectoKanban.Services;

namespace ProyectoKanban.Controllers
{
    [Authorize(Roles = MyConstants.RolAdmin)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var tareas = _context.Tareas.AsQueryable();

            if (fechaInicio.HasValue)
                tareas = tareas.Where(t => t.FechaEntrega >= fechaInicio.Value);

            if (fechaFin.HasValue)
                tareas = tareas.Where(t => t.FechaEntrega <= fechaFin.Value);

            var usuarios = await _userManager.Users.ToListAsync();

            var stats = usuarios.Select(user =>
            {
                var tareasUsuario = tareas.Where(t => t.UsuarioId == user.Id).ToList();

                return new UsuarioTareaStats
                {
                    Usuario = user.UserName,
                    Completadas = tareasUsuario.Count(t => t.Estado == "Hecho"),
                    Pendientes = tareasUsuario.Count(t => t.Estado != "Hecho")
                };
            }).Where(s => s.Completadas + s.Pendientes > 0).ToList();

            var model = new DashboardViewModel
            {
                TareasPorUsuario = stats,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            return View(model);
        }
    }
}
