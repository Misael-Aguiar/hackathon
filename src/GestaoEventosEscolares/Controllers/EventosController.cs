using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

[Authorize]
public class EventosController : Controller
{
    private readonly IConsultaEventoService _consultaEventoService;

    public EventosController(IConsultaEventoService consultaEventoService)
    {
        _consultaEventoService = consultaEventoService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var eventos = await _consultaEventoService.ListarVisiveisParaUsuarioAsync(User, cancellationToken);
        return View(eventos);
    }

    /// <summary>
    /// Demonstra a policy ProfessorDoEvento: admin acessa qualquer evento;
    /// professor só acessa se estiver em ProfessoresAutorizadosEvento.
    /// </summary>
    [Authorize(Policy = PoliticasAutorizacao.ProfessorDoEvento)]
    public IActionResult Gerenciar(int id)
    {
        ViewData["EventoId"] = id;
        return View();
    }
}
