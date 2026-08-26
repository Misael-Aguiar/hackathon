using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task PopularAsync(IServiceProvider servicos)
    {
        var contexto = servicos.GetRequiredService<ApplicationDbContext>();
        var usuarios = servicos.GetRequiredService<UserManager<Usuario>>();
        var perfis = servicos.GetRequiredService<RoleManager<IdentityRole>>();

        await GarantirPerfisAsync(perfis);

        var administrador = await GarantirUsuarioAsync(
            usuarios,
            rm: "0000001",
            nome: "Administrador do Sistema",
            email: "admin@eventos.escola",
            senha: "Senha@Admin123",
            perfil: PerfilUsuario.Administrador);

        var professor = await GarantirUsuarioAsync(
            usuarios,
            rm: "1000001",
            nome: "Maria Silva",
            email: "maria.silva@eventos.escola",
            senha: "Senha@Prof123",
            perfil: PerfilUsuario.Professor);

        var aluno = await GarantirUsuarioAsync(
            usuarios,
            rm: "2000001",
            nome: "João Santos",
            email: "joao.santos@eventos.escola",
            senha: "Senha@Aluno123",
            perfil: PerfilUsuario.Aluno);

        await GarantirEventoDemoAsync(contexto, administrador, professor, aluno);
    }

    private static async Task GarantirPerfisAsync(RoleManager<IdentityRole> perfis)
    {
        string[] nomes = [NomesPerfis.Administrador, NomesPerfis.Professor, NomesPerfis.Aluno];

        foreach (var nome in nomes)
        {
            if (!await perfis.RoleExistsAsync(nome))
            {
                await perfis.CreateAsync(new IdentityRole(nome));
            }
        }
    }

    private static async Task<Usuario> GarantirUsuarioAsync(
        UserManager<Usuario> usuarios,
        string rm,
        string nome,
        string email,
        string senha,
        PerfilUsuario perfil)
    {
        var existente = await usuarios.Users.FirstOrDefaultAsync(usuario => usuario.RM == rm);
        if (existente is not null)
        {
            return existente;
        }

        var novo = new Usuario
        {
            UserName = rm,
            RM = rm,
            NomeCompleto = nome,
            Email = email,
            EmailConfirmed = true,
            Perfil = perfil,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        var resultado = await usuarios.CreateAsync(novo, senha);
        if (!resultado.Succeeded)
        {
            var erros = string.Join("; ", resultado.Errors.Select(erro => erro.Description));
            throw new InvalidOperationException($"Falha ao criar usuário {rm}: {erros}");
        }

        await usuarios.AddToRoleAsync(novo, perfil.ToString());
        return novo;
    }

    private static async Task GarantirEventoDemoAsync(
        ApplicationDbContext contexto,
        Usuario administrador,
        Usuario professor,
        Usuario aluno)
    {
        if (await contexto.Eventos.AnyAsync())
        {
            return;
        }

        var evento = new Evento
        {
            Titulo = "Feira de Ciências 2026",
            Descricao = "Mostra anual de projetos científicos dos alunos do ensino médio.",
            DataInicio = DateTime.UtcNow.Date.AddDays(7).AddHours(13),
            DataFim = DateTime.UtcNow.Date.AddDays(7).AddHours(18),
            Local = "Auditório Principal",
            CargaHorariaHoras = 4,
            LimiteVagas = 80,
            Status = StatusEvento.Publicado,
            CriadoPorUsuarioId = administrador.Id,
            DataCriacao = DateTime.UtcNow
        };

        contexto.Eventos.Add(evento);
        await contexto.SaveChangesAsync();

        contexto.ProfessoresAutorizadosEvento.Add(new ProfessorAutorizadoEvento
        {
            EventoId = evento.Id,
            ProfessorId = professor.Id,
            AutorizadoPorUsuarioId = administrador.Id,
            DataAutorizacao = DateTime.UtcNow
        });

        contexto.Inscricoes.Add(new Inscricao
        {
            EventoId = evento.Id,
            AlunoId = aluno.Id,
            DataInscricao = DateTime.UtcNow,
            Status = StatusInscricao.Ativa
        });

        await contexto.SaveChangesAsync();
    }
}
