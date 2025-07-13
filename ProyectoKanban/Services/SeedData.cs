using Microsoft.AspNetCore.Identity;

namespace ProyectoKanban.Services
{
    public class SeedData
    {
        public static async Task CrearUsuarioAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string emailAdmin = "admin@kanban.com";
            string passwordAdmin = "Admin123!";
            string rolAdmin = "admin";
            string nombreAdmin = "Administrador"; // 👈 Nombre que se guardará en UserName

            // 1. Crear el rol si no existe
            if (!await roleManager.RoleExistsAsync(rolAdmin))
            {
                await roleManager.CreateAsync(new IdentityRole(rolAdmin));
            }

            // 2. Verificar si el usuario ya existe
            var usuarioAdmin = await userManager.FindByEmailAsync(emailAdmin);
            if (usuarioAdmin == null)
            {
                var nuevoUsuario = new IdentityUser
                {
                    UserName = nombreAdmin, // 👈 Aquí se guarda el nombre en UserName
                    Email = emailAdmin,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(nuevoUsuario, passwordAdmin);

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(nuevoUsuario, rolAdmin);
                }
                else
                {
                    throw new Exception("Error creando el usuario admin: " +
                        string.Join(", ", resultado.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
