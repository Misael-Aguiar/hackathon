using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Services;

public class GestaoUsuarioService : IGestaoUsuarioService
{
    private readonly ApplicationDbContext _contexto;
    private readonly UserManager<Usuario> _usuarios;

    public GestaoUsuarioService(ApplicationDbContext contexto, UserManager<Usuario> usuarios)
    {
        _contexto = contexto;
        _usuarios = usuarios;
    }

    public async Task<GestaoAlunosViewModel> ObterAlunosAsync(CancellationToken cancellationToken = default)
    {
        var alunos = await _contexto.Users
            .AsNoTracking()
            .Where(usuario => usuario.Perfil == PerfilUsuario.Aluno && usuario.Ativo)
            .OrderBy(usuario => usuario.NomeCompleto)
            .Select(usuario => new AlunoListagemViewModel
            {
                Id = usuario.Id,
                Nome = usuario.NomeCompleto,
                RM = usuario.RM,
                Sala = usuario.Sala ?? SalaTurma.DS1,
                TotalInscricoesAtivas = usuario.Inscricoes.Count(item => item.Status == StatusInscricao.Ativa)
            })
            .ToListAsync(cancellationToken);

        return new GestaoAlunosViewModel { Alunos = alunos };
    }

    public async Task<ResultadoCadastroUsuario> CadastrarAlunoAsync(
        CadastroAlunoViewModel modelo,
        CancellationToken cancellationToken = default)
    {
        var rm = NormalizarRm(modelo.RM);
        await GarantirRmDisponivelAsync(rm, cancellationToken);

        if (modelo.Sala is null)
        {
            throw new InvalidOperationException("Selecione a sala do aluno.");
        }

        var senha = GerarSenhaInicial(rm);
        var aluno = new Usuario
        {
            UserName = rm,
            RM = rm,
            NomeCompleto = modelo.Nome.Trim(),
            Email = MontarEmail(rm),
            EmailConfirmed = true,
            Perfil = PerfilUsuario.Aluno,
            Sala = modelo.Sala,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        await CriarIdentityAsync(aluno, senha, NomesPerfis.Aluno);
        return new ResultadoCadastroUsuario { Nome = aluno.NomeCompleto, RM = rm, SenhaInicial = senha };
    }

    public async Task ExcluirAlunoAsync(
        string id,
        ClaimsPrincipal administrador,
        CancellationToken cancellationToken = default)
    {
        var aluno = await ObterParaExclusaoAsync(id, PerfilUsuario.Aluno, administrador, cancellationToken);

        // Soft delete: o aluno permanece na tabela Usuarios para as FKs Restrict
        // de Inscricoes, Presencas e Certificados continuarem válidas.
        aluno.Ativo = false;

        // Inscrições sem check-in deixam de ocupar vaga. As que já têm presença
        // ficam ativas no histórico (presença e certificado não são apagados).
        var inscricoesSemPresenca = await _contexto.Inscricoes
            .Where(item =>
                item.AlunoId == aluno.Id
                && item.Status == StatusInscricao.Ativa
                && item.Presenca == null)
            .ToListAsync(cancellationToken);

        foreach (var inscricao in inscricoesSemPresenca)
        {
            inscricao.Status = StatusInscricao.Cancelada;
        }

        await _usuarios.UpdateSecurityStampAsync(aluno);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<GestaoProfessoresViewModel> ObterProfessoresAsync(CancellationToken cancellationToken = default)
    {
        var professores = await _contexto.Users
            .AsNoTracking()
            .Where(usuario => usuario.Perfil == PerfilUsuario.Professor && usuario.Ativo)
            .OrderBy(usuario => usuario.NomeCompleto)
            .Select(usuario => new ProfessorListagemViewModel
            {
                Id = usuario.Id,
                Nome = usuario.NomeCompleto,
                RM = usuario.RM,
                Telefone = usuario.Telefone ?? string.Empty,
                TotalEventosVinculados = usuario.EventosAutorizados.Count
            })
            .ToListAsync(cancellationToken);

        return new GestaoProfessoresViewModel { Professores = professores };
    }

    public async Task<ResultadoCadastroUsuario> CadastrarProfessorAsync(
        CadastroProfessorViewModel modelo,
        CancellationToken cancellationToken = default)
    {
        var rm = NormalizarRm(modelo.RM);
        await GarantirRmDisponivelAsync(rm, cancellationToken);

        var senha = GerarSenhaInicial(rm);
        var professor = new Usuario
        {
            UserName = rm,
            RM = rm,
            NomeCompleto = modelo.Nome.Trim(),
            Email = MontarEmail(rm),
            EmailConfirmed = true,
            PhoneNumber = modelo.Telefone.Trim(),
            Perfil = PerfilUsuario.Professor,
            Telefone = modelo.Telefone.Trim(),
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        await CriarIdentityAsync(professor, senha, NomesPerfis.Professor);
        return new ResultadoCadastroUsuario { Nome = professor.NomeCompleto, RM = rm, SenhaInicial = senha };
    }

    public async Task ExcluirProfessorAsync(
        string id,
        ClaimsPrincipal administrador,
        CancellationToken cancellationToken = default)
    {
        var professor = await ObterParaExclusaoAsync(id, PerfilUsuario.Professor, administrador, cancellationToken);

        professor.Ativo = false;

        // Remove só o vínculo de permissão. Eventos criados e presenças validadas
        // continuam apontando para este usuário (DeleteBehavior.Restrict).
        var vinculos = await _contexto.ProfessoresAutorizadosEvento
            .Where(item => item.ProfessorId == professor.Id)
            .ToListAsync(cancellationToken);
        _contexto.ProfessoresAutorizadosEvento.RemoveRange(vinculos);

        await _usuarios.UpdateSecurityStampAsync(professor);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario> ObterParaExclusaoAsync(
        string id,
        PerfilUsuario perfilEsperado,
        ClaimsPrincipal administrador,
        CancellationToken cancellationToken)
    {
        var usuario = await _contexto.Users
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        if (usuario.Id == administrador.ObterId())
        {
            throw new InvalidOperationException("Você não pode excluir o próprio usuário.");
        }

        if (usuario.Perfil != perfilEsperado)
        {
            throw new InvalidOperationException("Este usuário não pertence a esta aba.");
        }

        if (!usuario.Ativo)
        {
            throw new InvalidOperationException("Este usuário já foi excluído.");
        }

        return usuario;
    }

    private async Task GarantirRmDisponivelAsync(string rm, CancellationToken cancellationToken)
    {
        var existe = await _contexto.Users.AnyAsync(item => item.RM == rm, cancellationToken);
        if (existe)
        {
            throw new InvalidOperationException("Já existe um cadastro com este RM (inclusive inativo).");
        }
    }

    private async Task CriarIdentityAsync(Usuario usuario, string senha, string role)
    {
        var resultado = await _usuarios.CreateAsync(usuario, senha);
        if (!resultado.Succeeded)
        {
            var erros = string.Join(" ", resultado.Errors.Select(erro => erro.Description));
            throw new InvalidOperationException(erros);
        }

        await _usuarios.AddToRoleAsync(usuario, role);
    }

    private static string NormalizarRm(string rm) => rm.Trim().ToUpperInvariant();

    private static string MontarEmail(string rm) => $"{rm.ToLowerInvariant()}@eventos.escola";

    /// <summary>
    /// Senha provisória única por RM, compatível com as regras do Identity.
    /// </summary>
    private static string GerarSenhaInicial(string rm) => $"Nexus@{rm}!";
}
