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

        if (!PayloadQrInscricao.TentarLer(payloadQr, out var eventoIdNoQr, out var codigoQr))
        {
            return ValidacaoPresencaResultado.Falha("QR code inválido.");
        }

        if (eventoIdNoQr is int eventoLido && eventoLido != eventoId)
        {
            return ValidacaoPresencaResultado.Falha("Este QR code é de outro evento.");
        }

        var inscricao = await _contexto.Inscricoes
            .Include(item => item.Aluno)
            .Include(item => item.Presenca)
            .FirstOrDefaultAsync(item => item.CodigoQr == codigoQr, cancellationToken);

        if (inscricao is null)
        {
            return ValidacaoPresencaResultado.Falha("QR code inválido.");
        }

        if (inscricao.EventoId != eventoId)
        {
            return ValidacaoPresencaResultado.Falha("Este QR code é de outro evento.");
        }

        if (inscricao.Status != StatusInscricao.Ativa)
        {
            return ValidacaoPresencaResultado.Falha("QR code inválido.");
        }

        if (inscricao.Presenca is not null)
        {
            return ValidacaoPresencaResultado.Falha("QR code já utilizado.");
        }

        var horario = DateTime.Now;
        _contexto.Presencas.Add(new Presenca
        {
            EventoId = eventoId,
            InscricaoId = inscricao.Id,
            AlunoId = inscricao.AlunoId,
            ValidadoPorUsuarioId = validadorId,
            DataValidacao = horario,
            CodigoQrUtilizado = PayloadQrInscricao.Montar(eventoId, codigoQr)
        });

        try
        {
            await _contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ValidacaoPresencaResultado.Falha("QR code já utilizado.");
        }

        return ValidacaoPresencaResultado.Ok(inscricao.Aluno.NomeCompleto, inscricao.Aluno.RM, horario);
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
