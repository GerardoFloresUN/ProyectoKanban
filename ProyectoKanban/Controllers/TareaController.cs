using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoKanban.Entities;
using ProyectoKanban.Models;

namespace ProyectoKanban.Controllers
{
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
                UsuarioNombre = usuarioEmail
            });
        }

        return View(lista);
    }

        public async Task<IActionResult> TareaAdd()
        {
            ViewBag.Usuarios = new SelectList(await _userManager.Users.ToListAsync(), "Id", "Email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TareaAdd(TareaModel model)
        {
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
                Estado = model.Estado,
                UsuarioId = model.UsuarioId
            };

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            return RedirectToAction("TareaList");
        }

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
                UsuarioId = tarea.UsuarioId
            };

            ViewBag.Usuarios = new SelectList(await _userManager.Users.ToListAsync(), "Id", "Email", model.UsuarioId);
            return View(model);
        }

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

            _context.Update(tarea);
            await _context.SaveChangesAsync();

            return RedirectToAction("TareaList");
        }
    }
}
