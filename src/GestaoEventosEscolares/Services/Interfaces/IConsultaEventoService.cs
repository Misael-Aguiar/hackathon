using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IConsultaEventoService
{
    Task<IReadOnlyList<EventoResumoViewModel>> ListarVisiveisParaUsuarioAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default);
}
