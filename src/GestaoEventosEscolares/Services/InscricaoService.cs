using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Data.Consultas;
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

        if (existente is not null && existente.Status == StatusInscricao.Ativa)
        {
            return existente.Id;
        }

        // Fecha no DataInicio: quem já está inscrito manteve o QR acima; nova inscrição não entra.
        if (!EventoConsultas.InscricaoAberta(evento.DataInicio, DateTime.Now))
        {
            throw new InvalidOperationException("As inscrições encerraram no início do evento.");
        }

        if (existente is not null)
        {
            existente.Status = StatusInscricao.Ativa;
            existente.DataInscricao = DateTime.UtcNow;
            // Novo GUID: o QR cancelado anterior continua inválido.
            existente.CodigoQr = PayloadQrInscricao.GerarCodigo();
            existente.CodigoCheckIn = await GerarCheckInUnicoAsync(cancellationToken);

            await _contexto.SaveChangesAsync(cancellationToken);
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
            CodigoQr = PayloadQrInscricao.GerarCodigo(),
            CodigoCheckIn = await GerarCheckInUnicoAsync(cancellationToken)
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

    public async Task CancelarAsync(
        int inscricaoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        if (!usuario.EhAluno())
        {
            throw new InvalidOperationException("Apenas o aluno pode cancelar a própria inscrição.");
        }

        var alunoId = usuario.ObterId()
            ?? throw new InvalidOperationException("Usuário autenticado sem identificador.");

        var inscricao = await _contexto.Inscricoes
            .Include(item => item.Evento)
            .Include(item => item.Presenca)
            .FirstOrDefaultAsync(item => item.Id == inscricaoId, cancellationToken)
            ?? throw new InvalidOperationException("Inscrição não encontrada.");

        if (inscricao.AlunoId != alunoId)
        {
            throw new UnauthorizedAccessException("Esta inscrição não pertence ao seu usuário.");
        }

        if (inscricao.Status != StatusInscricao.Ativa)
        {
            throw new InvalidOperationException("Esta inscrição já foi cancelada.");
        }

        if (inscricao.Presenca is not null)
        {
            throw new InvalidOperationException("Não é possível cancelar após a confirmação de presença.");
        }

        if (!EventoConsultas.InscricaoAberta(inscricao.Evento.DataInicio, DateTime.Now))
        {
            throw new InvalidOperationException("O período de inscrição já encerrou.");
        }

        inscricao.Status = StatusInscricao.Cancelada;
        inscricao.CodigoQr = PayloadQrInscricao.GerarCodigo();
        inscricao.CodigoCheckIn = await GerarCheckInUnicoAsync(cancellationToken);
        await _contexto.SaveChangesAsync(cancellationToken);
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
    {
        var ativa = inscricao.Status == StatusInscricao.Ativa;
        var codigo = inscricao.CodigoQr ?? string.Empty;

        return new ConfirmacaoInscricaoViewModel
        {
            InscricaoId = inscricao.Id,
            EventoId = inscricao.EventoId,
            TituloEvento = inscricao.Evento.Titulo,
            NomeAluno = inscricao.Aluno.NomeCompleto,
            RM = inscricao.Aluno.RM,
            DataInicio = inscricao.Evento.DataInicio,
            Local = inscricao.Evento.Local,
            PayloadQr = ativa ? PayloadQrInscricao.Montar(inscricao.EventoId, codigo) : string.Empty,
            PresencaConfirmada = inscricao.Presenca is not null,
            PodeBaixarCertificado = inscricao.Presenca is not null,
            CertificadoEmitido = inscricao.Certificado is not null,
            QrDisponivel = ativa,
            PodeCancelar = ativa
                && inscricao.Presenca is null
                && EventoConsultas.InscricaoAberta(inscricao.Evento.DataInicio, DateTime.Now),
            CodigoCheckIn = ativa ? inscricao.CodigoCheckIn : string.Empty
        };
    }

    private async Task<string> GerarCheckInUnicoAsync(CancellationToken cancellationToken)
    {
        for (var tentativa = 0; tentativa < 12; tentativa++)
        {
            var candidato = GeradorCodigoCheckIn.Gerar();
            var existe = await _contexto.Inscricoes
                .AnyAsync(item => item.CodigoCheckIn == candidato, cancellationToken);
            if (!existe)
            {
                return candidato;
            }
        }

        throw new InvalidOperationException("Não foi possível gerar um código de check-in único.");
    }
}
