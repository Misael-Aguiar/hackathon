using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Models.Enums;
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

    public async Task<bool> PossuiPermissaoAsync(
        string professorId,
        int eventoId,
        PermissaoEvento permissao,
        CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.ProfessoresAutorizadosEvento
            .AsNoTracking()
            .Where(vinculo => vinculo.ProfessorId == professorId && vinculo.EventoId == eventoId);

        consulta = AplicarPermissao(consulta, permissao);
        return await consulta.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> ObterIdsEventosAutorizadosAsync(
        string professorId,
        PermissaoEvento permissao = PermissaoEvento.QualquerVinculo,
        CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.ProfessoresAutorizadosEvento
            .AsNoTracking()
            .Where(vinculo => vinculo.ProfessorId == professorId);

        consulta = AplicarPermissao(consulta, permissao);

        return await consulta
            .Select(vinculo => vinculo.EventoId)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Models.Entidades.ProfessorAutorizadoEvento> AplicarPermissao(
        IQueryable<Models.Entidades.ProfessorAutorizadoEvento> consulta,
        PermissaoEvento permissao)
    {
        return permissao switch
        {
            PermissaoEvento.Editar => consulta.Where(vinculo => vinculo.PodeEditarEvento),
            PermissaoEvento.AcessarPresenca => consulta.Where(vinculo => vinculo.PodeAcessarPresenca),
            _ => consulta
        };
    }
}
