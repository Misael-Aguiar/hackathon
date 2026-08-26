using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Services;

public class InscricaoService : IInscricaoService
{
    private readonly ApplicationDbContext _contexto;

    public InscricaoService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<int> InscreverAsync(
        int eventoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        if (!usuario.EhAluno())
        {
            throw new InvalidOperationException("Apenas alunos podem se inscrever em eventos.");
        }

        var alunoId = usuario.ObterId()
            ?? throw new InvalidOperationException("Usuário autenticado sem identificador.");

        var evento = await _contexto.Eventos
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken)
            ?? throw new InvalidOperationException("Evento não encontrado.");

        if (evento.Status is not (StatusEvento.Publicado or StatusEvento.EmAndamento))
        {
            throw new InvalidOperationException("Este evento não está aberto para inscrições.");
        }

        var existente = await _contexto.Inscricoes
            .FirstOrDefaultAsync(
                item => item.EventoId == eventoId && item.AlunoId == alunoId,
                cancellationToken);

        if (existente is not null)
        {
            if (existente.Status == StatusInscricao.Cancelada)
            {
                existente.Status = StatusInscricao.Ativa;
                existente.DataInscricao = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(existente.CodigoQr))
                {
                    existente.CodigoQr = PayloadQrInscricao.GerarCodigo();
                }

                await _contexto.SaveChangesAsync(cancellationToken);
            }

            return existente.Id;
        }

        if (evento.LimiteVagas is int limite)
        {
            var totalAtivas = await _contexto.Inscricoes.CountAsync(
                item => item.EventoId == eventoId && item.Status == StatusInscricao.Ativa,
                cancellationToken);

            if (totalAtivas >= limite)
            {
                throw new InvalidOperationException("As vagas deste evento acabaram.");
            }
        }

        var inscricao = new Inscricao
        {
            EventoId = eventoId,
            AlunoId = alunoId,
            DataInscricao = DateTime.UtcNow,
            Status = StatusInscricao.Ativa,
            CodigoQr = PayloadQrInscricao.GerarCodigo()
        };

        _contexto.Inscricoes.Add(inscricao);
        await _contexto.SaveChangesAsync(cancellationToken);
        return inscricao.Id;
    }

    public async Task<ConfirmacaoInscricaoViewModel?> ObterConfirmacaoAsync(
        int inscricaoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var inscricao = await _contexto.Inscricoes
            .AsNoTracking()
            .Include(item => item.Evento)
            .Include(item => item.Aluno)
            .Include(item => item.Presenca)
            .Include(item => item.Certificado)
            .FirstOrDefaultAsync(item => item.Id == inscricaoId, cancellationToken);

        if (inscricao is null || !PodeVerInscricao(inscricao, usuario))
        {
            return null;
        }

        return Mapear(inscricao);
    }

    public async Task<IReadOnlyList<ConfirmacaoInscricaoViewModel>> ListarDoAlunoAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var alunoId = usuario.ObterId();
        if (string.IsNullOrWhiteSpace(alunoId))
        {
            return [];
        }

        var inscricoes = await _contexto.Inscricoes
            .AsNoTracking()
            .Include(item => item.Evento)
            .Include(item => item.Aluno)
            .Include(item => item.Presenca)
            .Include(item => item.Certificado)
            .Where(item => item.AlunoId == alunoId && item.Status == StatusInscricao.Ativa)
            .OrderBy(item => item.Evento.DataInicio)
            .ToListAsync(cancellationToken);

        return inscricoes.Select(Mapear).ToList();
    }

    private static bool PodeVerInscricao(Inscricao inscricao, ClaimsPrincipal usuario)
    {
        if (usuario.EhAdministrador())
        {
            return true;
        }

        return usuario.EhAluno() && inscricao.AlunoId == usuario.ObterId();
    }

    private static ConfirmacaoInscricaoViewModel Mapear(Inscricao inscricao)
        => new()
        {
            InscricaoId = inscricao.Id,
            EventoId = inscricao.EventoId,
            TituloEvento = inscricao.Evento.Titulo,
            NomeAluno = inscricao.Aluno.NomeCompleto,
            RM = inscricao.Aluno.RM,
            DataInicio = inscricao.Evento.DataInicio,
            Local = inscricao.Evento.Local,
            PayloadQr = PayloadQrInscricao.Montar(inscricao.EventoId, inscricao.CodigoQr),
            PresencaConfirmada = inscricao.Presenca is not null,
            PodeBaixarCertificado = inscricao.Presenca is not null,
            CertificadoEmitido = inscricao.Certificado is not null
        };
}
