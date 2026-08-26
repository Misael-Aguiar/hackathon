using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

[Authorize]
public class InscricoesController : Controller
{
    private readonly IInscricaoService _inscricaoService;
    private readonly IGeradorQrCodeService _geradorQrCode;

    public InscricoesController(IInscricaoService inscricaoService, IGeradorQrCodeService geradorQrCode)
    {
        _inscricaoService = inscricaoService;
        _geradorQrCode = geradorQrCode;
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.SomenteAluno)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrever(int eventoId, CancellationToken cancellationToken)
    {
        try
        {
            var inscricaoId = await _inscricaoService.InscreverAsync(eventoId, User, cancellationToken);
            TempData["MensagemSucesso"] = "Inscrição confirmada. Apresente o QR Code na entrada do evento.";
            return RedirectToAction(nameof(Confirmacao), new { id = inscricaoId });
        }
        catch (InvalidOperationException excecao)
        {
            TempData["MensagemErro"] = excecao.Message;
            return RedirectToAction("Details", "Eventos", new { id = eventoId });
        }
    }

    public async Task<IActionResult> Confirmacao(int id, CancellationToken cancellationToken)
    {
        var modelo = await _inscricaoService.ObterConfirmacaoAsync(id, User, cancellationToken);
        if (modelo is null)
        {
            return NotFound();
        }

        return View(modelo);
    }

    /// <summary>PNG do QR. Inscrição cancelada não gera imagem (código já foi rotacionado).</summary>
    public async Task<IActionResult> QrCode(int id, CancellationToken cancellationToken)
    {
        var modelo = await _inscricaoService.ObterConfirmacaoAsync(id, User, cancellationToken);
        if (modelo is null || !modelo.QrDisponivel || string.IsNullOrWhiteSpace(modelo.PayloadQr))
        {
            return NotFound();
        }

        var png = _geradorQrCode.GerarPng(modelo.PayloadQr);
        return File(png, "image/png");
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.SomenteAluno)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _inscricaoService.CancelarAsync(id, User, cancellationToken);
            TempData["MensagemSucesso"] = "Inscrição cancelada. A vaga foi liberada e o QR Code deixou de valer.";
            return RedirectToAction(nameof(Minhas));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException excecao)
        {
            TempData["MensagemErro"] = excecao.Message;
            return RedirectToAction(nameof(Confirmacao), new { id });
        }
    }

    [Authorize(Policy = PoliticasAutorizacao.SomenteAluno)]
    public async Task<IActionResult> Minhas(CancellationToken cancellationToken)
    {
        var inscricoes = await _inscricaoService.ListarDoAlunoAsync(User, cancellationToken);
        return View(inscricoes);
    }
}
