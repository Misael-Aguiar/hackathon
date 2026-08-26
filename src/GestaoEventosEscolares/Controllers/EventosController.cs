using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

[Authorize]
public class EventosController : Controller
{
    private readonly IConsultaEventoService _consultaEventoService;
    private readonly IGestaoEventoService _gestaoEventoService;

    public EventosController(
        IConsultaEventoService consultaEventoService,
        IGestaoEventoService gestaoEventoService)
    {
        _consultaEventoService = consultaEventoService;
        _gestaoEventoService = gestaoEventoService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var eventos = await _consultaEventoService.ListarVisiveisParaUsuarioAsync(User, cancellationToken);
        return View(eventos);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var detalhe = await _consultaEventoService.ObterDetalheAsync(id, User, cancellationToken);
        if (detalhe is null)
        {
            return NotFound();
        }

        return View(detalhe);
    }

    [Authorize(Policy = PoliticasAutorizacao.ProfessorOuAdministrador)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var modelo = await _gestaoEventoService.MontarFormularioNovoAsync(User, cancellationToken);
        return View(modelo);
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.ProfessorOuAdministrador)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Create(FormularioEventoViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await _gestaoEventoService.PreencherOpcoesDeFormularioAsync(modelo, User, cancellationToken);
            return View(modelo);
        }

        try
        {
            var id = await _gestaoEventoService.CriarAsync(modelo, User, cancellationToken);
            TempData["MensagemSucesso"] = "Evento criado.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (InvalidOperationException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            await _gestaoEventoService.PreencherOpcoesDeFormularioAsync(modelo, User, cancellationToken);
            return View(modelo);
        }
    }

    // Policy lê o {id} da rota: admin sempre; professor só com PodeEditarEvento.
    [Authorize(Policy = PoliticasAutorizacao.ProfessorPodeEditarEvento)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var modelo = await _gestaoEventoService.MontarFormularioEdicaoAsync(id, User, cancellationToken);
        if (modelo is null)
        {
            return NotFound();
        }

        return View(modelo);
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.ProfessorPodeEditarEvento)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Edit(int id, FormularioEventoViewModel modelo, CancellationToken cancellationToken)
    {
        modelo.Id = id;

        if (!ModelState.IsValid)
        {
            await _gestaoEventoService.PreencherOpcoesDeFormularioAsync(modelo, User, cancellationToken);
            return View(modelo);
        }

        try
        {
            await _gestaoEventoService.AtualizarAsync(modelo, User, cancellationToken);
            TempData["MensagemSucesso"] = "Evento atualizado.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (InvalidOperationException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            await _gestaoEventoService.PreencherOpcoesDeFormularioAsync(modelo, User, cancellationToken);
            return View(modelo);
        }
    }

    [Authorize(Policy = PoliticasAutorizacao.SomenteAdministrador)]
    public async Task<IActionResult> GerenciarPermissoes(int id, CancellationToken cancellationToken)
    {
        var modelo = await _gestaoEventoService.ObterPermissoesAsync(id, cancellationToken);
        if (modelo is null)
        {
            return NotFound();
        }

        return View(modelo);
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.SomenteAdministrador)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GerenciarPermissoes(int id, PermissoesEventoViewModel modelo, CancellationToken cancellationToken)
    {
        modelo.EventoId = id;

        try
        {
            await _gestaoEventoService.SalvarPermissoesAsync(modelo, User, cancellationToken);
            TempData["MensagemSucesso"] = "Permissões atualizadas.";
            return RedirectToAction(nameof(GerenciarPermissoes), new { id });
        }
        catch (InvalidOperationException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            var recarregado = await _gestaoEventoService.ObterPermissoesAsync(id, cancellationToken);
            return View(recarregado ?? modelo);
        }
    }

    [Authorize(Policy = PoliticasAutorizacao.ProfessorDoEvento)]
    public IActionResult Gerenciar(int id) => RedirectToAction(nameof(Edit), new { id });
}
