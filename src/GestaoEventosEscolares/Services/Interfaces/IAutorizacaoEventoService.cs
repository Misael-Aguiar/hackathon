using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IAutorizacaoEventoService
{
    Task<bool> PossuiPermissaoAsync(
        string professorId,
        int eventoId,
        PermissaoEvento permissao,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ObterIdsEventosAutorizadosAsync(
        string professorId,
        PermissaoEvento permissao = PermissaoEvento.QualquerVinculo,
        CancellationToken cancellationToken = default);
}
