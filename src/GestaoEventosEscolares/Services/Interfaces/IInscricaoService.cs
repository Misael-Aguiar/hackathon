using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IInscricaoService
{
    Task<int> InscreverAsync(int eventoId, ClaimsPrincipal usuario, CancellationToken cancellationToken = default);

    Task<ConfirmacaoInscricaoViewModel?> ObterConfirmacaoAsync(
        int inscricaoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConfirmacaoInscricaoViewModel>> ListarDoAlunoAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);
}
