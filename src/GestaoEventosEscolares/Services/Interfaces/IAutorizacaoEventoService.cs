namespace GestaoEventosEscolares.Services.Interfaces;

public interface IAutorizacaoEventoService
{
    Task<bool> ProfessorEstaAutorizadoAsync(string professorId, int eventoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ObterIdsEventosAutorizadosAsync(string professorId, CancellationToken cancellationToken = default);
}
