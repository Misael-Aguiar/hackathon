using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IConsultaEventoService
{
    Task<IReadOnlyList<EventoCarrosselItemViewModel>> ListarParaCarrosselAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoResumoViewModel>> ListarVisiveisParaUsuarioAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);

    Task<DetalheEventoViewModel?> ObterDetalheAsync(
        int eventoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);
}
