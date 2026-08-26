using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

[Authorize]
public class PainelController : Controller
{
    private readonly IConsultaEventoService _consultaEventoService;

    public PainelController(IConsultaEventoService consultaEventoService)
    {
        _consultaEventoService = consultaEventoService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var eventos = await _consultaEventoService.ListarVisiveisParaUsuarioAsync(User, cancellationToken);

        var modelo = new PainelViewModel
        {
            NomeCompleto = User.ObterNomeCompleto(),
            RM = User.ObterRm(),
            Perfil = User.ObterPerfil(),
            TotalEventosVisiveis = eventos.Count,
            EventosDestaque = eventos.Take(5).ToList()
        };

        return View(modelo);
    }
}
