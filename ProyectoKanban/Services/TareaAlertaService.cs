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
                                    $@"
                                    <div style='font-family: Arial, sans-serif; background-color: #f8f9fa; padding: 20px; border-radius: 10px; max-width: 600px; margin: auto;'>
                                        <h2 style='color: #dc3545;'>⏰ ¡Atención!</h2>
                                        <p>Hola <strong>{user.UserName}</strong>,</p>
                                        <p>La siguiente tarea asignada a ti está próxima a vencer:</p>
                                        <table style='width: 100%; margin-top: 10px; border-collapse: collapse;'>
                                            <tr>
                                                <td style='padding: 8px; border: 1px solid #dee2e6; background-color: #e9ecef;'><strong>Nombre de la tarea</strong></td>
                                                <td style='padding: 8px; border: 1px solid #dee2e6;'>{tarea.Nombre}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px; border: 1px solid #dee2e6; background-color: #e9ecef;'><strong>Fecha de entrega</strong></td>
                                                <td style='padding: 8px; border: 1px solid #dee2e6;'>{tarea.FechaEntrega:dd/MM/yyyy}</td>
                                            </tr>
                                        </table>
                                        <p style='margin-top: 20px;'>Te recomendamos completar esta tarea antes de la fecha límite.</p>
                                        <p style='font-size: 0.9em; color: #6c757d;'>Este mensaje ha sido generado automáticamente por el sistema Kanban.</p>
                                    </div>"
                                );
                                tarea.AlertaEnviada = true;
                            }
                        }
                        else if (tarea.AlertaEnviada && hoy < fechaAlerta)
                        {
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
