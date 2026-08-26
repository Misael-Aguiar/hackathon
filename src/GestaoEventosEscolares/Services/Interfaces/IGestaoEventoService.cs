using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IGestaoEventoService
{
    Task<FormularioEventoViewModel> MontarFormularioNovoAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task<FormularioEventoViewModel?> MontarFormularioEdicaoAsync(
        int eventoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task<int> CriarAsync(
        FormularioEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task AtualizarAsync(
        FormularioEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task<PermissoesEventoViewModel?> ObterPermissoesAsync(
        int eventoId,
        CancellationToken cancellationToken = default);

    Task SalvarPermissoesAsync(
        PermissoesEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task PreencherOpcoesDeFormularioAsync(
        FormularioEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task<ConfirmacaoExclusaoEventoViewModel?> ObterConfirmacaoExclusaoAsync(
        int eventoId,
        CancellationToken cancellationToken = default);

    Task ExcluirAsync(int eventoId, CancellationToken cancellationToken = default);
}
