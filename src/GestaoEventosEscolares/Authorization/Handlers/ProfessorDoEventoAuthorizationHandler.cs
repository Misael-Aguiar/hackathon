using GestaoEventosEscolares.Authorization.Requirements;
using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace GestaoEventosEscolares.Authorization.Handlers;

/// <summary>
/// Administrador passa sempre. Professor precisa do vínculo e, quando exigido, da flag de permissão.
/// </summary>
public class ProfessorDoEventoAuthorizationHandler : AuthorizationHandler<ProfessorDoEventoRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAutorizacaoEventoService _autorizacaoEventoService;

    public ProfessorDoEventoAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IAutorizacaoEventoService autorizacaoEventoService)
    {
        _httpContextAccessor = httpContextAccessor;
        _autorizacaoEventoService = autorizacaoEventoService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProfessorDoEventoRequirement requirement)
    {
        if (context.User.IsInRole(NomesPerfis.Administrador))
        {
            context.Succeed(requirement);
            return;
        }

        if (!context.User.IsInRole(NomesPerfis.Professor))
        {
            return;
        }

        var eventoId = ObterEventoIdDaRota();
        if (eventoId is null)
        {
            return;
        }

        var usuarioId = context.User.ObterId();
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return;
        }

        var autorizado = await _autorizacaoEventoService.PossuiPermissaoAsync(
            usuarioId,
            eventoId.Value,
            requirement.Permissao);

        if (autorizado)
        {
            context.Succeed(requirement);
        }
    }

    private int? ObterEventoIdDaRota()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        var rota = httpContext.GetRouteData()?.Values ?? httpContext.Request.RouteValues;
        if (TentarLerInteiro(rota, "eventoId", out var eventoId) || TentarLerInteiro(rota, "id", out eventoId))
        {
            return eventoId;
        }

        if (int.TryParse(httpContext.Request.Query["eventoId"], out eventoId))
        {
            return eventoId;
        }

        return null;
    }

    private static bool TentarLerInteiro(RouteValueDictionary valores, string chave, out int valor)
    {
        valor = 0;
        return valores.TryGetValue(chave, out var bruto)
               && bruto is not null
               && int.TryParse(bruto.ToString(), out valor);
    }
}
