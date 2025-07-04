using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoKanban.Entities;

namespace ProyectoKanban.Services
{
    public class TareaAlertaService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TareaAlertaService> _logger;

        public TareaAlertaService(IServiceProvider serviceProvider, ILogger<TareaAlertaService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                    var hoy = DateTime.Today;

                    var tareas = await context.Tareas
                        .Where(t => t.DiasAntesAlerta > 0)
                        .ToListAsync();

                    foreach (var tarea in tareas)
                    {
                        var fechaAlerta = tarea.FechaEntrega.AddDays(-tarea.DiasAntesAlerta);

                        if (!tarea.AlertaEnviada && hoy >= fechaAlerta)
                        {
                            var user = await userManager.FindByIdAsync(tarea.UsuarioId);
                            if (user != null)
                            {
                                await emailSender.SendEmailAsync(
                                    user.Email,
                                    $"⏰ Alerta: Tarea '{tarea.Nombre}' próxima a vencer",
                                    $"Hola, tu tarea <b>{tarea.Nombre}</b> vence el {tarea.FechaEntrega:dd/MM/yyyy}."
                                );
                                tarea.AlertaEnviada = true;
                            }
                        }
                        else if (tarea.AlertaEnviada && hoy < fechaAlerta)
                        {
                            // 🔄 Si cambia la fecha y la alerta ya fue enviada, permitir reenviarla
                            tarea.AlertaEnviada = false;
                        }
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("🔁 Revisión de tareas completada a las {hora}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error durante la revisión de tareas.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
