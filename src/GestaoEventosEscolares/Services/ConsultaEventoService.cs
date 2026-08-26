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

    public async Task<IReadOnlyList<EventoCarrosselItemViewModel>> ListarParaCarrosselAsync(
        CancellationToken cancellationToken = default)
    {
        return await _contexto.Eventos
            .AsNoTracking()
            .Where(evento => evento.Status == StatusEvento.Publicado || evento.Status == StatusEvento.EmAndamento)
            .OrderBy(evento => evento.DataInicio)
            .Select(evento => new EventoCarrosselItemViewModel
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                Subtitulo = evento.Subtitulo,
                CaminhoImagem = evento.CaminhoImagem,
                DataInicio = evento.DataInicio
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventoResumoViewModel>> ListarVisiveisParaUsuarioAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.Eventos.AsNoTracking();
        var idsEdicao = new HashSet<int>();
        var idsPresenca = new HashSet<int>();
        var podeGerenciarPermissoes = usuario.EhAdministrador();

        if (usuario.EhAdministrador())
        {
        }
        else if (usuario.EhProfessor())
        {
            var usuarioId = usuario.ObterId() ?? string.Empty;
            var idsVinculo = await _autorizacaoEventoService
                .ObterIdsEventosAutorizadosAsync(usuarioId, PermissaoEvento.QualquerVinculo, cancellationToken);
            idsEdicao = (await _autorizacaoEventoService
                    .ObterIdsEventosAutorizadosAsync(usuarioId, PermissaoEvento.Editar, cancellationToken))
                .ToHashSet();
            idsPresenca = (await _autorizacaoEventoService
                    .ObterIdsEventosAutorizadosAsync(usuarioId, PermissaoEvento.AcessarPresenca, cancellationToken))
                .ToHashSet();

            consulta = consulta.Where(evento =>
                idsVinculo.Contains(evento.Id)
                || evento.Status == StatusEvento.Publicado
                || evento.Status == StatusEvento.EmAndamento);
        }
        else
        {
            consulta = consulta.Where(evento =>
                evento.Status == StatusEvento.Publicado || evento.Status == StatusEvento.EmAndamento);
        }

        var itens = await consulta
            .OrderBy(evento => evento.DataInicio)
            .Select(evento => new EventoResumoViewModel
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                Subtitulo = evento.Subtitulo,
                CaminhoImagem = evento.CaminhoImagem,
                DataInicio = evento.DataInicio,
                DataFim = evento.DataFim,
                Local = evento.Local,
                Status = evento.Status,
                TotalInscritos = evento.Inscricoes.Count(inscricao => inscricao.Status == StatusInscricao.Ativa)
            })
            .ToListAsync(cancellationToken);

        foreach (var item in itens)
        {
            item.PodeGerenciarPermissoes = podeGerenciarPermissoes;
            item.PodeEditar = podeGerenciarPermissoes || idsEdicao.Contains(item.Id);
            item.PodeValidarPresenca = podeGerenciarPermissoes || idsPresenca.Contains(item.Id);
        }

        return itens;
    }

    public async Task<DetalheEventoViewModel?> ObterDetalheAsync(
        int eventoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var evento = await _contexto.Eventos
            .AsNoTracking()
            .Include(item => item.ProfessoresAutorizados)
            .ThenInclude(vinculo => vinculo.Professor)
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken);

        if (evento is null || !PodeVisualizar(evento, usuario))
        {
            return null;
        }

        var usuarioId = usuario.ObterId();
        var vinculo = evento.ProfessoresAutorizados
            .FirstOrDefault(item => item.ProfessorId == usuarioId);

        var inscricaoAluno = usuario.EhAluno() && usuarioId is not null
            ? await _contexto.Inscricoes.AsNoTracking()
                .Include(item => item.Presenca)
                .FirstOrDefaultAsync(
                    item => item.EventoId == eventoId && item.AlunoId == usuarioId && item.Status == StatusInscricao.Ativa,
                    cancellationToken)
            : null;

        var eventoAberto = evento.Status is StatusEvento.Publicado or StatusEvento.EmAndamento;
        string? motivoBloqueio = null;
        if (usuario.EhAluno() && inscricaoAluno is null)
        {
            if (!eventoAberto)
            {
                motivoBloqueio = "Este evento não está aberto para inscrições.";
            }
            else if (evento.LimiteVagas is int limite)
            {
                var totalAtivas = await _contexto.Inscricoes.CountAsync(
                    item => item.EventoId == eventoId && item.Status == StatusInscricao.Ativa,
                    cancellationToken);
                if (totalAtivas >= limite)
                {
                    motivoBloqueio = "As vagas deste evento acabaram.";
                }
            }
        }

        return new DetalheEventoViewModel
        {
            Id = evento.Id,
            Titulo = evento.Titulo,
            Subtitulo = evento.Subtitulo,
            Descricao = evento.Descricao,
            Objetivo = evento.Objetivo,
            InformacoesAdicionais = evento.InformacoesAdicionais,
            CaminhoImagem = evento.CaminhoImagem,
            Local = evento.Local,
            DataInicio = evento.DataInicio,
            DataFim = evento.DataFim,
            Status = evento.Status,
            ProfessoresResponsaveis = evento.ProfessoresAutorizados
                .Where(item => item.PodeEditarEvento)
                .Select(item => item.Professor.NomeCompleto)
                .OrderBy(nome => nome)
                .ToList(),
            PodeEditar = usuario.EhAdministrador() || (vinculo?.PodeEditarEvento ?? false),
            PodeGerenciarPermissoes = usuario.EhAdministrador(),
            PodeValidarPresenca = usuario.EhAdministrador() || (vinculo?.PodeAcessarPresenca ?? false),
            JaInscrito = inscricaoAluno is not null,
            InscricaoId = inscricaoAluno?.Id,
            PodeBaixarCertificado = inscricaoAluno?.Presenca is not null,
            PodeInscrever = usuario.EhAluno() && inscricaoAluno is null && motivoBloqueio is null,
            PrecisaLoginAluno = usuario.Identity?.IsAuthenticated != true && eventoAberto,
            MotivoBloqueioInscricao = motivoBloqueio
        };
    }

    private static bool PodeVisualizar(Models.Entidades.Evento evento, ClaimsPrincipal usuario)
    {
        if (evento.Status is StatusEvento.Publicado or StatusEvento.EmAndamento)
        {
            return true;
        }

        if (usuario.EhAdministrador())
        {
            return true;
        }

        var usuarioId = usuario.ObterId();
        return usuario.EhProfessor()
               && evento.ProfessoresAutorizados.Any(vinculo => vinculo.ProfessorId == usuarioId);
    }
}
