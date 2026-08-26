using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace GestaoEventosEscolares.Services;

public class CertificadoService : ICertificadoService
{
    private readonly ApplicationDbContext _contexto;

    public CertificadoService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<PerfilAlunoViewModel> ObterPerfilAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var alunoId = usuario.ObterId()
            ?? throw new InvalidOperationException("Usuário autenticado sem identificador.");

        var aluno = await _contexto.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == alunoId, cancellationToken)
            ?? throw new InvalidOperationException("Aluno não encontrado.");

        var inscricoes = await _contexto.Inscricoes
            .AsNoTracking()
            .Include(item => item.Evento)
            .Include(item => item.Presenca)
            .Include(item => item.Certificado)
            .Where(item => item.AlunoId == alunoId && item.Status == StatusInscricao.Ativa)
            .OrderByDescending(item => item.Evento.DataInicio)
            .ToListAsync(cancellationToken);

        var historico = inscricoes.Select(MapearParticipacao).ToList();

        return new PerfilAlunoViewModel
        {
            NomeCompleto = aluno.NomeCompleto,
            RM = aluno.RM,
            Email = aluno.Email ?? string.Empty,
            TotalInscricoes = historico.Count,
            TotalPresencas = historico.Count(item => item.Status is StatusParticipacao.Presente or StatusParticipacao.CertificadoEmitido),
            TotalCertificados = historico.Count(item => item.Status == StatusParticipacao.CertificadoEmitido),
            Historico = historico
        };
    }

    public async Task<ArquivoCertificado> BaixarAsync(
        int inscricaoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var alunoId = usuario.ObterId()
            ?? throw new InvalidOperationException("Usuário autenticado sem identificador.");

        var inscricao = await _contexto.Inscricoes
            .Include(item => item.Evento)
            .Include(item => item.Aluno)
            .Include(item => item.Presenca)
            .Include(item => item.Certificado)
            .FirstOrDefaultAsync(item => item.Id == inscricaoId, cancellationToken)
            ?? throw new InvalidOperationException("Inscrição não encontrada.");

        if (inscricao.AlunoId != alunoId && !usuario.EhAdministrador())
        {
            throw new UnauthorizedAccessException("Este certificado não pertence ao seu usuário.");
        }

        if (!usuario.EhAluno() && !usuario.EhAdministrador())
        {
            throw new UnauthorizedAccessException("Apenas o aluno pode baixar o certificado.");
        }

        if (inscricao.Presenca is null)
        {
            throw new InvalidOperationException("O certificado só é liberado após a confirmação de presença.");
        }

        var certificado = inscricao.Certificado ?? await EmitirAsync(inscricao, cancellationToken);

        var dados = new DadosCertificadoPdf
        {
            NomeAluno = inscricao.Aluno.NomeCompleto,
            RM = inscricao.Aluno.RM,
            TituloEvento = inscricao.Evento.Titulo,
            CargaHorariaHoras = ResolverCargaHoraria(inscricao.Evento),
            DataEvento = inscricao.Evento.DataInicio,
            DataEmissao = certificado.DataEmissao,
            CodigoVerificacao = certificado.CodigoVerificacao
        };

        var pdf = new CertificadoParticipacaoDocument(dados).GeneratePdf();
        var nomeArquivo = $"certificado-{SanitizarNome(inscricao.Evento.Titulo)}.pdf";

        return new ArquivoCertificado
        {
            Conteudo = pdf,
            NomeArquivo = nomeArquivo
        };
    }

    private async Task<Certificado> EmitirAsync(Inscricao inscricao, CancellationToken cancellationToken)
    {
        var novo = new Certificado
        {
            EventoId = inscricao.EventoId,
            InscricaoId = inscricao.Id,
            AlunoId = inscricao.AlunoId,
            CodigoVerificacao = GerarCodigoVerificacao(),
            DataEmissao = DateTime.Now
        };

        _contexto.Certificados.Add(novo);

        try
        {
            await _contexto.SaveChangesAsync(cancellationToken);
            return novo;
        }
        catch (DbUpdateException)
        {
            _contexto.Entry(novo).State = EntityState.Detached;
            return await _contexto.Certificados
                .FirstAsync(item => item.InscricaoId == inscricao.Id, cancellationToken);
        }
    }

    private static ParticipacaoEventoViewModel MapearParticipacao(Inscricao inscricao)
    {
        var status = inscricao.Certificado is not null
            ? StatusParticipacao.CertificadoEmitido
            : inscricao.Presenca is not null
                ? StatusParticipacao.Presente
                : StatusParticipacao.Inscrito;

        return new ParticipacaoEventoViewModel
        {
            InscricaoId = inscricao.Id,
            EventoId = inscricao.EventoId,
            TituloEvento = inscricao.Evento.Titulo,
            DataInicio = inscricao.Evento.DataInicio,
            CargaHorariaHoras = ResolverCargaHoraria(inscricao.Evento),
            Status = status,
            PodeBaixarCertificado = inscricao.Presenca is not null
        };
    }

    private static int ResolverCargaHoraria(Evento evento)
        => evento.CargaHorariaHoras > 0
            ? evento.CargaHorariaHoras
            : Math.Max(1, (int)Math.Ceiling((evento.DataFim - evento.DataInicio).TotalHours));

    private static string GerarCodigoVerificacao()
        => Convert.ToHexString(Guid.NewGuid().ToByteArray())[..12];

    private static string SanitizarNome(string titulo)
    {
        var caracteres = titulo
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(caracteres).Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(slug) ? "evento" : slug;
    }
}
