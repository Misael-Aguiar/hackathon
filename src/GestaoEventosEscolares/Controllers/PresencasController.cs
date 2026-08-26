using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

[Authorize(Policy = PoliticasAutorizacao.ProfessorPodeAcessarPresenca)]
public class PresencasController : Controller
{
    private readonly IPresencaService _presencaService;

    public PresencasController(IPresencaService presencaService)
    {
        _presencaService = presencaService;
    }

    // {id} = EventoId. Policy ProfessorPodeAcessarPresenca lê a rota.
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
    {
        var tabela = await _presencaService.ObterTabelaAsync(id, cancellationToken);
        if (tabela is null)
        {
            return NotFound();
        }

        return View(tabela);
    }

    public async Task<IActionResult> Validar(int id, CancellationToken cancellationToken)
    {
        var pagina = await _presencaService.ObterPaginaValidacaoAsync(id, cancellationToken);
        if (pagina is null)
        {
            return NotFound();
        }

        return View(pagina);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validar(int id, [FromBody] LeituraQrViewModel? leitura, CancellationToken cancellationToken)
        {
            var codigo = leitura?.Codigo ?? string.Empty;
            var resultado = await _presencaService.ValidarLeituraAsync(id, codigo, User, cancellationToken);
            return Json(resultado);
        }
}
