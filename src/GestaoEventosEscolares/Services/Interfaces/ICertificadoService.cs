using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface ICertificadoService
{
    Task<PerfilAlunoViewModel> ObterPerfilAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gera (ou reemite) o PDF somente se a presença da inscrição estiver confirmada.
    /// </summary>
    Task<ArquivoCertificado> BaixarAsync(
        int inscricaoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);
}
