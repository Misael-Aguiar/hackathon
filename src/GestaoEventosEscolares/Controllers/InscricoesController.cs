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

    /// <summary>PNG do QR Code. Só o aluno dono da inscrição (ou admin) enxerga.</summary>
    public async Task<IActionResult> QrCode(int id, CancellationToken cancellationToken)
    {
        var modelo = await _inscricaoService.ObterConfirmacaoAsync(id, User, cancellationToken);
        if (modelo is null)
        {
            return NotFound();
        }

        var png = _geradorQrCode.GerarPng(modelo.PayloadQr);
        return File(png, "image/png");
    }

    [Authorize(Policy = PoliticasAutorizacao.SomenteAluno)]
    public async Task<IActionResult> Minhas(CancellationToken cancellationToken)
    {
        var inscricoes = await _inscricaoService.ListarDoAlunoAsync(User, cancellationToken);
        return View(inscricoes);
    }
}
