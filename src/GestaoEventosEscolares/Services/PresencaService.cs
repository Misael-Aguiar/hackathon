using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Services;

public class PresencaService : IPresencaService
{
    private readonly ApplicationDbContext _contexto;

    public PresencaService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<ValidacaoPresencaResultado> ValidarLeituraAsync(
        int eventoId,
        string payloadQr,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var validadorId = usuario.ObterId();
        if (string.IsNullOrWhiteSpace(validadorId))
        {
            return ValidacaoPresencaResultado.Falha("Não foi possível identificar quem está validando.");
        }

        var resolucao = await ResolverInscricaoAsync(eventoId, payloadQr, cancellationToken);
        if (resolucao.Erro is not null)
        {
            return ValidacaoPresencaResultado.Falha(resolucao.Erro);
        }

        var inscricao = resolucao.Inscricao!;

        if (inscricao.Status != StatusInscricao.Ativa)
        {
            return ValidacaoPresencaResultado.Falha("Código inválido.");
        }

        if (inscricao.Presenca is not null)
        {
            return ValidacaoPresencaResultado.Falha("Código já utilizado.");
        }

        var horario = DateTime.Now;
        _contexto.Presencas.Add(new Presenca
        {
            EventoId = eventoId,
            InscricaoId = inscricao.Id,
            AlunoId = inscricao.AlunoId,
            ValidadoPorUsuarioId = validadorId,
            DataValidacao = horario,
            CodigoQrUtilizado = PayloadQrInscricao.Montar(eventoId, inscricao.CodigoQr)
        });

        try
        {
            await _contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ValidacaoPresencaResultado.Falha("Código já utilizado.");
        }

        return ValidacaoPresencaResultado.Ok(inscricao.Aluno.NomeCompleto, inscricao.Aluno.RM, horario);
    }

    /// <summary>
    /// QR (GEE:evento:guid) e código curto caem neste método; a validação depois é a mesma.
    /// </summary>
    private async Task<(Inscricao? Inscricao, string? Erro)> ResolverInscricaoAsync(
        int eventoId,
        string bruto,
        CancellationToken cancellationToken)
    {
        Inscricao? inscricao = null;

        if (PayloadQrInscricao.TentarLer(bruto, out var eventoIdNoQr, out var codigoQr))
        {
            if (eventoIdNoQr != eventoId)
            {
                return (null, "Este código é de outro evento.");
            }

            inscricao = await _contexto.Inscricoes
                .Include(item => item.Aluno)
                .Include(item => item.Presenca)
                .FirstOrDefaultAsync(item => item.CodigoQr == codigoQr, cancellationToken);
        }
        else if (GeradorCodigoCheckIn.TentarNormalizar(bruto, out var codigoCurto))
        {
            inscricao = await _contexto.Inscricoes
                .Include(item => item.Aluno)
                .Include(item => item.Presenca)
                .FirstOrDefaultAsync(item => item.CodigoCheckIn == codigoCurto, cancellationToken);
        }
        else
        {
            return (null, "Código inválido.");
        }

        if (inscricao is null)
        {
            return (null, "Código inválido.");
        }

        if (inscricao.EventoId != eventoId)
        {
            return (null, "Este código é de outro evento.");
        }

        return (inscricao, null);
    }

    public async Task<TabelaPresencaViewModel?> ObterTabelaAsync(
        int eventoId,
        CancellationToken cancellationToken = default)
    {
        var evento = await _contexto.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken);

        if (evento is null)
        {
            return null;
        }

        var inscricoes = await _contexto.Inscricoes
            .AsNoTracking()
            .Include(item => item.Aluno)
            .Include(item => item.Presenca)
            .ThenInclude(presenca => presenca!.ValidadoPor)
            .Where(item => item.EventoId == eventoId && item.Status == StatusInscricao.Ativa)
            .OrderBy(item => item.Aluno.NomeCompleto)
            .ToListAsync(cancellationToken);

        var presentes = inscricoes
            .Where(item => item.Presenca is not null)
            .Select(item => new AlunoPresenteViewModel
            {
                NomeCompleto = item.Aluno.NomeCompleto,
                RM = item.Aluno.RM,
                DataValidacao = item.Presenca!.DataValidacao,
                ValidadoPor = item.Presenca.ValidadoPor.NomeCompleto,
                ValidadoPorRm = item.Presenca.ValidadoPor.RM
            })
            .OrderBy(item => item.DataValidacao)
            .ToList();

        var pendentes = inscricoes
            .Where(item => item.Presenca is null)
            .Select(item => new AlunoPendenteViewModel
            {
                NomeCompleto = item.Aluno.NomeCompleto,
                RM = item.Aluno.RM
            })
            .ToList();

        return new TabelaPresencaViewModel
        {
            EventoId = evento.Id,
            TituloEvento = evento.Titulo,
            TotalInscritos = inscricoes.Count,
            TotalPresentes = presentes.Count,
            Presentes = presentes,
            Pendentes = pendentes
        };
    }

    public async Task<ValidarPresencaPaginaViewModel?> ObterPaginaValidacaoAsync(
        int eventoId,
        CancellationToken cancellationToken = default)
    {
        var evento = await _contexto.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken);

        if (evento is null)
        {
            return null;
        }

        return new ValidarPresencaPaginaViewModel
        {
            EventoId = evento.Id,
            TituloEvento = evento.Titulo
        };
    }
}
