using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IPresencaService
{
    Task<ValidacaoPresencaResultado> ValidarLeituraAsync(
        int eventoId,
        string payloadQr,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task<TabelaPresencaViewModel?> ObterTabelaAsync(
        int eventoId,
        CancellationToken cancellationToken = default);

    Task<ValidarPresencaPaginaViewModel?> ObterPaginaValidacaoAsync(
        int eventoId,
        CancellationToken cancellationToken = default);
}
