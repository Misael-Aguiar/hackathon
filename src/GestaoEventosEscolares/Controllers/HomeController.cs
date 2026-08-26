using System.Diagnostics;
using GestaoEventosEscolares.Models;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

public class HomeController : Controller
{
    private readonly IConsultaEventoService _consultaEventoService;

    public HomeController(IConsultaEventoService consultaEventoService)
    {
        _consultaEventoService = consultaEventoService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var eventos = await _consultaEventoService.ListarParaCarrosselAsync(cancellationToken);
        return View(eventos);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
