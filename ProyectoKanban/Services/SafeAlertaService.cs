using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoKanban;
using ProyectoKanban.Services;

public class TareaAlertaService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TareaAlertaService> _logger;

    public TareaAlertaService(IServiceProvider serviceProvider, ILogger<TareaAlertaService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // Método que puede reutilizarse
    public async Task RevisarAlertasAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var hoy = DateTime.Today;
        var tareas = await context.Tareas
            .Where(t => !t.AlertaEnviada && t.DiasAntesAlerta > 0)
            .ToListAsync();

        foreach (var tarea in tareas)
        {
            var fechaAlerta = tarea.FechaEntrega.AddDays(-tarea.DiasAntesAlerta);
            if (hoy >= fechaAlerta)
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
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("🔁 Revisión de tareas completada a las {hora}", DateTime.Now);
    }

    // Lógica ejecutada por el BackgroundService
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RevisarAlertasAsync();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
