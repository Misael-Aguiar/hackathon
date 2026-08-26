using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Services;

public class AutorizacaoEventoService : IAutorizacaoEventoService
{
    private readonly ApplicationDbContext _contexto;

    public AutorizacaoEventoService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<bool> ProfessorEstaAutorizadoAsync(
        string professorId,
        int eventoId,
        CancellationToken cancellationToken = default)
    {
        return await _contexto.ProfessoresAutorizadosEvento
            .AsNoTracking()
            .AnyAsync(
                vinculo => vinculo.ProfessorId == professorId && vinculo.EventoId == eventoId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<int>> ObterIdsEventosAutorizadosAsync(
        string professorId,
        CancellationToken cancellationToken = default)
    {
        return await _contexto.ProfessoresAutorizadosEvento
            .AsNoTracking()
            .Where(vinculo => vinculo.ProfessorId == professorId)
            .Select(vinculo => vinculo.EventoId)
            .ToListAsync(cancellationToken);
    }
}
