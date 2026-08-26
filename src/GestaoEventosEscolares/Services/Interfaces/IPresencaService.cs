using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IPresencaService
{
    /// <summary>
    /// Check-in único: o texto pode ser o payload do QR (GEE:evento:guid) ou o apelido curto.
    /// Os dois resolvem a mesma inscrição e seguem as mesmas regras.
    /// </summary>
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
