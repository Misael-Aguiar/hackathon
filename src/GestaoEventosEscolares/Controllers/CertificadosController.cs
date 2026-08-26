using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

[Authorize(Policy = PoliticasAutorizacao.SomenteAluno)]
public class CertificadosController : Controller
{
    private readonly ICertificadoService _certificadoService;

    public CertificadosController(ICertificadoService certificadoService)
    {
        _certificadoService = certificadoService;
    }

    /// <summary>
    /// {id} = InscricaoId. Só libera PDF se existir presença confirmada.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        try
        {
            var arquivo = await _certificadoService.BaixarAsync(id, User, cancellationToken);
            return File(arquivo.Conteudo, "application/pdf", arquivo.NomeArquivo);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException excecao)
        {
            TempData["MensagemErro"] = excecao.Message;
            return RedirectToAction("Index", "Perfil");
        }
    }
}
