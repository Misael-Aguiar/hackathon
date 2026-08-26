using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Services;

public class ConsultaEventoService : IConsultaEventoService
{
    private readonly ApplicationDbContext _contexto;
    private readonly IAutorizacaoEventoService _autorizacaoEventoService;

    public ConsultaEventoService(
        ApplicationDbContext contexto,
        IAutorizacaoEventoService autorizacaoEventoService)
    {
        _contexto = contexto;
        _autorizacaoEventoService = autorizacaoEventoService;
    }

    public async Task<IReadOnlyList<EventoResumoViewModel>> ListarVisiveisParaUsuarioAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.Eventos.AsNoTracking();

        if (usuario.EhAdministrador())
        {
            // Administrador vê todos os eventos.
        }
        else if (usuario.EhProfessor())
        {
            var usuarioId = usuario.ObterId();
            var idsAutorizados = await _autorizacaoEventoService
                .ObterIdsEventosAutorizadosAsync(usuarioId ?? string.Empty, cancellationToken);

            consulta = consulta.Where(evento => idsAutorizados.Contains(evento.Id));
        }
        else
        {
            consulta = consulta.Where(evento =>
                evento.Status == StatusEvento.Publicado || evento.Status == StatusEvento.EmAndamento);
        }

        return await consulta
            .OrderBy(evento => evento.DataInicio)
            .Select(evento => new EventoResumoViewModel
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                DataInicio = evento.DataInicio,
                DataFim = evento.DataFim,
                Local = evento.Local,
                Status = evento.Status,
                TotalInscritos = evento.Inscricoes.Count(inscricao => inscricao.Status == StatusInscricao.Ativa)
            })
            .ToListAsync(cancellationToken);
    }
}
