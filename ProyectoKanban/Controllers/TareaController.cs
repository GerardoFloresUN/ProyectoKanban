using ProyectoKanban.Entities;
using ProyectoKanban.Models;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoKanban.Controllers
{
    public class TareaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TareaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult TareaList()
        {
            var lista = _context.Tareas.Select(t => new TareaModel
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                FechaInicio = t.FechaInicio,
                FechaEntrega = t.FechaEntrega,
                Estado = t.Estado
            }).ToList();

            return View(lista);
        }

        public IActionResult TareaAdd() => View();

        [HttpPost]
        public IActionResult TareaAdd(TareaModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var tarea = new Tarea
            {
                Id = Guid.NewGuid(),
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                FechaInicio = model.FechaInicio,
                FechaEntrega = model.FechaEntrega,
                Estado = model.Estado
            };

            _context.Tareas.Add(tarea);
            _context.SaveChanges();

            return RedirectToAction("TareaList");
        }

        [HttpGet]
        public IActionResult TareaEdit(Guid id)
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
                Estado = tarea.Estado
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult TareaEdit(TareaModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var tarea = _context.Tareas.FirstOrDefault(t => t.Id == model.Id);
            if (tarea == null) return RedirectToAction("TareaList");

            tarea.Nombre = model.Nombre;
            tarea.Descripcion = model.Descripcion;
            tarea.FechaInicio = model.FechaInicio;
            tarea.FechaEntrega = model.FechaEntrega;
            tarea.Estado = model.Estado;

            _context.Tareas.Update(tarea);
            _context.SaveChanges();

            return RedirectToAction("TareaList");
        }

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
                Estado = tarea.Estado
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult TareaDeleted(TareaModel model)
        {
            var tarea = _context.Tareas.FirstOrDefault(t => t.Id == model.Id);
            if (tarea == null) return RedirectToAction("TareaList");

            _context.Tareas.Remove(tarea);
            _context.SaveChanges();

            return RedirectToAction("TareaList");
        }
    }
}
