using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoKanban.Models;
using ProyectoKanban.Services;

namespace ProyectoKanban.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public UsuarioController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._context = context;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Kanban", "Tarea");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = new IdentityUser()
            {
                Email = model.Email,
                UserName = model.Nombre // 👈 Aquí guardamos el "nombre" como UserName
            };

            var resultado = await userManager.CreateAsync(usuario, model.Password);

            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Kanban", "Tarea");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login(string mensaje = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Kanban", "Tarea");
            }

            if (mensaje != null)
            {
                ViewData["mensaje"] = mensaje;
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 👇 Aún usamos el email para login (aunque el "nombre" está en UserName)
            var resultado = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Kanban", "Tarea");
            }

            ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login", "Usuario");
        }

        public async Task<IActionResult> Listado(string confirmed = null, string remove = null)
        {
            var usuarios = _context.Users.ToList();
            var lista = new List<UsuarioViewModel>();

            foreach (var user in usuarios)
            {
                var esAdmin = await userManager.IsInRoleAsync(user, MyConstants.RolAdmin);

                lista.Add(new UsuarioViewModel
                {
                    Email = user.Email,
                    EsAdmin = esAdmin,
                    Nombre = user.UserName // 👈 Mostrar el "nombre" (UserName)
                });
            }

            var model = new UsuariosListadoViewModel
            {
                Usuarios = lista,
                Mensaje = !string.IsNullOrEmpty(confirmed) ? confirmed : remove
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> HacerAdmin(string email)
        {
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return NotFound();

            await userManager.AddToRoleAsync(usuario, MyConstants.RolAdmin);
            return RedirectToAction("Listado", new { confirmed = $"Rol asignado correctamente a {email}" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoverAdmin(string email)
        {
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return NotFound();

            var usuarioActual = User.Identity?.Name;
            if (usuario.Email == usuarioActual)
            {
                TempData["Error"] = "No puedes quitarte el rol de admin a ti mismo.";
                return RedirectToAction("Listado");
            }

            await userManager.RemoveFromRoleAsync(usuario, MyConstants.RolAdmin);
            return RedirectToAction("Listado", new { remove = $"Rol removido correctamente a {email}" });
        }

        [HttpPost]
[Authorize(Roles = MyConstants.RolAdmin)]
public async Task<IActionResult> EliminarUsuario(string email)
{
    var usuario = await userManager.FindByEmailAsync(email);
    if (usuario == null) return NotFound();

    // Verificar si tiene tareas asignadas
    var tieneTareas = _context.Tareas.Any(t => t.UsuarioId == usuario.Id);

    if (tieneTareas)
    {
        TempData["Error"] = $"El usuario '{usuario.UserName}' tiene tareas asignadas y no se puede eliminar.";
        return RedirectToAction("Listado");
    }

    await userManager.DeleteAsync(usuario);
    return RedirectToAction("Listado", new { remove = $"Usuario {email} eliminado correctamente" });
}

    }
}
