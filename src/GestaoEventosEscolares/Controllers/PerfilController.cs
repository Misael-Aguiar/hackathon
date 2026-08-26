using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

[Authorize(Policy = PoliticasAutorizacao.SomenteAluno)]
public class PerfilController : Controller
{
    private readonly ICertificadoService _certificadoService;

    public PerfilController(ICertificadoService certificadoService)
    {
        _certificadoService = certificadoService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var perfil = await _certificadoService.ObterPerfilAsync(User, cancellationToken);
        return View(perfil);
    }
}
