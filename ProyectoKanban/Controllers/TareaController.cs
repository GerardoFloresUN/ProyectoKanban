using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoKanban.Entities;
using ProyectoKanban.Models;

namespace ProyectoKanban.Controllers
{
    [Authorize]
    public class TareaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TareaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> TareaList()
        {
            var tareas = await _context.Tareas.ToListAsync();
            var lista = new List<TareaModel>();

            foreach (var tarea in tareas)
            {
                string? usuarioEmail = null;
                if (!string.IsNullOrEmpty(tarea.UsuarioId))
                {
                    var usuario = await _userManager.FindByIdAsync(tarea.UsuarioId);
                    usuarioEmail = usuario?.Email;
                }

                lista.Add(new TareaModel
                {
                    Id = tarea.Id,
                    Nombre = tarea.Nombre,
                    Descripcion = tarea.Descripcion,
                    FechaInicio = tarea.FechaInicio,
                    FechaEntrega = tarea.FechaEntrega,
                    Estado = tarea.Estado,
                    UsuarioId = tarea.UsuarioId,
                    UsuarioNombre = usuarioEmail,
                    Orden = tarea.Orden,
                    DiasAntesAlerta = tarea.DiasAntesAlerta,
                    AlertaEnviada = tarea.AlertaEnviada
                });
            }

            return View(lista);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TareaAdd()
        {
            ViewBag.Usuarios = new SelectList(await _userManager.Users.ToListAsync(), "Id", "Email");
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> TareaAdd(TareaModel model)
        {
            model.Estado = "Por hacer"; 

            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = new SelectList(await _userManager.Users.ToListAsync(), "Id", "Email");
                return View(model);
            }

            var tarea = new Tarea
            {
                Id = Guid.NewGuid(),
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                FechaInicio = model.FechaInicio,
                FechaEntrega = model.FechaEntrega,
                Estado = "Por hacer",
                UsuarioId = model.UsuarioId,
                Orden = model.Orden,
                DiasAntesAlerta = model.DiasAntesAlerta,
                AlertaEnviada = false
            };

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            return RedirectToAction("TareaList");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TareaEdit(Guid id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null) return RedirectToAction("TareaList");

            var model = new TareaModel
            {
                Id = tarea.Id,
                Nombre = tarea.Nombre,
                Descripcion = tarea.Descripcion,
                FechaInicio = tarea.FechaInicio,
                FechaEntrega = tarea.FechaEntrega,
                Estado = tarea.Estado,
                UsuarioId = tarea.UsuarioId,
                Orden = tarea.Orden,
                DiasAntesAlerta = tarea.DiasAntesAlerta,
                AlertaEnviada = tarea.AlertaEnviada
            };

            ViewBag.Usuarios = new SelectList(await _userManager.Users.ToListAsync(), "Id", "Email", model.UsuarioId);
            return View(model);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> TareaEdit(TareaModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = new SelectList(await _userManager.Users.ToListAsync(), "Id", "Email", model.UsuarioId);
                return View(model);
            }

            var tarea = await _context.Tareas.FindAsync(model.Id);
            if (tarea == null) return RedirectToAction("TareaList");

            tarea.Nombre = model.Nombre;
            tarea.Descripcion = model.Descripcion;
            tarea.FechaInicio = model.FechaInicio;
            tarea.FechaEntrega = model.FechaEntrega;
            tarea.Estado = model.Estado;
            tarea.UsuarioId = model.UsuarioId;
            tarea.Orden = model.Orden;
            tarea.DiasAntesAlerta = model.DiasAntesAlerta;
            tarea.AlertaEnviada = false; // Se reinicia al editar

            _context.Update(tarea);
            await _context.SaveChangesAsync();

            return RedirectToAction("TareaList");
        }

        [Authorize(Roles = "admin")]
        public IActionResult TareaDeleted(Guid id)
        {
            var tarea = _context.Tareas.FirstOrDefault(t => t.Id == id);
            if (tarea == null) return RedirectToAction("TareaList");

            var model = new TareaModel
            {
                Id = tarea.Id,
                Nombre = tarea.Nombre,
                Descripcion = tarea.Descripcion,
                FechaInicio = tarea.FechaInicio,
                FechaEntrega = tarea.FechaEntrega,
                Estado = tarea.Estado,
                DiasAntesAlerta = tarea.DiasAntesAlerta
            };

            return View(model);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult TareaDeleted(TareaModel model)
        {
            var tarea = _context.Tareas.FirstOrDefault(t => t.Id == model.Id);
            if (tarea == null) return RedirectToAction("TareaList");

            _context.Tareas.Remove(tarea);
            _context.SaveChanges();

            return RedirectToAction("TareaList");
        }

        // 🟦 Tablero Kanban
        public async Task<IActionResult> Kanban()
        {
            var tareas = await _context.Tareas
                .OrderBy(t => t.Estado)
                .ThenBy(t => t.Orden)
                .ToListAsync();

            // Mostrar alertas en la vista Kanban
            var hoy = DateTime.Today;
            ViewBag.Alertas = tareas
                .Where(t => !t.AlertaEnviada && t.DiasAntesAlerta > 0 && hoy >= t.FechaEntrega.AddDays(-t.DiasAntesAlerta))
                .ToList();

            var lista = new List<TareaModel>();
            foreach (var tarea in tareas)
            {
                string? usuarioEmail = null;
                if (!string.IsNullOrEmpty(tarea.UsuarioId))
                {
                    var usuario = await _userManager.FindByIdAsync(tarea.UsuarioId);
                    usuarioEmail = usuario?.Email;
                }

                lista.Add(new TareaModel
                {
                    Id = tarea.Id,
                    Nombre = tarea.Nombre,
                    Estado = tarea.Estado,
                    UsuarioNombre = usuarioEmail,
                    Orden = tarea.Orden,
                    DiasAntesAlerta = tarea.DiasAntesAlerta,
                    AlertaEnviada = tarea.AlertaEnviada
                });
            }

            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstadoTarea([FromBody] EstadoTareaModel model)
        {
            if (!Guid.TryParse(model.Id, out var tareaId)) return BadRequest();

            var tarea = await _context.Tareas.FindAsync(tareaId);
            if (tarea == null) return NotFound();

            tarea.Estado = model.Estado;
            tarea.Orden = model.Orden;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // ✅ Acción para marcar alerta como vista manualmente
        [HttpPost]
        public async Task<IActionResult> MarcarAlertaVista(Guid id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                tarea.AlertaEnviada = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Kanban");
        }

        public class EstadoTareaModel
        {
            public string Id { get; set; }
            public string Estado { get; set; }
            public int Orden { get; set; }
        }


        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ConfigurarAlerta(Guid id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null) return RedirectToAction("Kanban");

            return View(tarea);
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> ConfigurarAlerta(Guid id, int DiasAntesAlerta)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null) return RedirectToAction("Kanban");

            tarea.DiasAntesAlerta = DiasAntesAlerta;
            tarea.AlertaEnviada = false;

            await _context.SaveChangesAsync();
            return RedirectToAction("Kanban");
        }

    }
}
