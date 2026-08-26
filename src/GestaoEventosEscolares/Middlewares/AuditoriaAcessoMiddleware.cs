using GestaoEventosEscolares.Extensions;

namespace GestaoEventosEscolares.Middlewares;

/// <summary>
/// Registra acessos autenticados (RM, perfil e rota) para auditoria básica.
/// </summary>
public class AuditoriaAcessoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditoriaAcessoMiddleware> _logger;

    public AuditoriaAcessoMiddleware(RequestDelegate next, ILogger<AuditoriaAcessoMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation(
                "Acesso autenticado. RM={RM} Perfil={Perfil} {Metodo} {Caminho}",
                context.User.ObterRm(),
                context.User.ObterPerfil(),
                context.Request.Method,
                context.Request.Path);
        }

        await _next(context);
    }
}

public static class AuditoriaAcessoMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditoriaAcesso(this IApplicationBuilder app)
        => app.UseMiddleware<AuditoriaAcessoMiddleware>();
}
