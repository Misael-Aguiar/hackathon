using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Data.Consultas;
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
        var agora = DateTime.Now;

        return await _contexto.Eventos
            .AsNoTracking()
            .Where(evento => evento.Status == StatusEvento.Publicado || evento.Status == StatusEvento.EmAndamento)
            .OndeDentroDaJanelaDeExibicao(agora)
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
        var agora = DateTime.Now;
        var consulta = _contexto.Eventos.AsNoTracking();
        var idsEdicao = new HashSet<int>();
        var idsPresenca = new HashSet<int>();
        var podeGerenciarPermissoes = usuario.EhAdministrador();

        if (usuario.EhAdministrador())
        {
            // Admin: todos os eventos, sem a janela de 1 semana.
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

            consulta = consulta.OndeDentroDaJanelaDeExibicao(agora);
        }
        else
        {
            consulta = consulta
                .Where(evento => evento.Status == StatusEvento.Publicado || evento.Status == StatusEvento.EmAndamento)
                .OndeDentroDaJanelaDeExibicao(agora);
        }

        var itens = await consulta
            .OrdenarPorDataRecente()
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
            item.PodeExcluir = podeGerenciarPermissoes;
        }

        return itens;
    }

    public async Task<DetalheEventoViewModel?> ObterDetalheAsync(
        int eventoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTime.Now;
        var evento = await _contexto.Eventos
            .AsNoTracking()
            .Include(item => item.ProfessoresAutorizados)
            .ThenInclude(vinculo => vinculo.Professor)
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken);

        if (evento is null)
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

        if (!PodeVisualizar(evento, usuario, vinculo is not null, inscricaoAluno is not null, agora))
        {
            return null;
        }

        var eventoPublicado = evento.Status is StatusEvento.Publicado or StatusEvento.EmAndamento;
        var inscricaoAberta = EventoConsultas.InscricaoAberta(evento.DataInicio, agora);
        string? motivoBloqueio = null;
        if (usuario.EhAluno() && inscricaoAluno is null)
        {
            if (!eventoPublicado)
            {
                motivoBloqueio = "Este evento não está aberto para inscrições.";
            }
            else if (!inscricaoAberta)
            {
                motivoBloqueio = "As inscrições encerraram no início do evento.";
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
            PodeExcluir = usuario.EhAdministrador(),
            JaInscrito = inscricaoAluno is not null,
            InscricaoId = inscricaoAluno?.Id,
            PodeBaixarCertificado = inscricaoAluno?.Presenca is not null,
            PodeCancelarInscricao = inscricaoAluno is not null
                && inscricaoAluno.Presenca is null
                && inscricaoAberta,
            PodeInscrever = usuario.EhAluno() && inscricaoAluno is null && motivoBloqueio is null,
            PrecisaLoginAluno = usuario.Identity?.IsAuthenticated != true && eventoPublicado && inscricaoAberta,
            MotivoBloqueioInscricao = motivoBloqueio
        };
    }

    /// <summary>
    /// Listagem expira em 7 dias; detalhe continua para admin, aluno inscrito (histórico) e professor autorizado.
    /// </summary>
    private static bool PodeVisualizar(
        Models.Entidades.Evento evento,
        ClaimsPrincipal usuario,
        bool professorAutorizado,
        bool alunoInscrito,
        DateTime agora)
    {
        if (usuario.EhAdministrador() || alunoInscrito || professorAutorizado)
        {
            return true;
        }

        var naJanela = evento.DataInicio >= EventoConsultas.LimiteExibicaoListagem(agora);
        return naJanela && evento.Status is StatusEvento.Publicado or StatusEvento.EmAndamento;
    }
}
