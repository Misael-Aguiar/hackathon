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

        var professorAuxiliar = await GarantirUsuarioAsync(
            usuarios,
            rm: "1000002",
            nome: "Carlos Oliveira",
            email: "carlos.oliveira@eventos.escola",
            senha: "Senha@Prof123",
            perfil: PerfilUsuario.Professor);

        var aluno = await GarantirUsuarioAsync(
            usuarios,
            rm: "2000001",
            nome: "João Santos",
            email: "joao.santos@eventos.escola",
            senha: "Senha@Aluno123",
            perfil: PerfilUsuario.Aluno);

        await GarantirEventoDemoAsync(contexto, administrador, professor, professorAuxiliar, aluno);
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
        Usuario professorAuxiliar,
        Usuario aluno)
    {
        if (await contexto.Eventos.AnyAsync())
        {
        await ComplementarEventoExistenteAsync(contexto, professor, professorAuxiliar);
            return;
        }

        var feira = new Evento
        {
            Titulo = "Feira de Ciências 2026",
            Subtitulo = "Projetos do ensino médio em exposição",
            Descricao = "Mostra anual de projetos científicos dos alunos do ensino médio, com bancas e visitação das famílias.",
            Objetivo = "Estimular a investigação científica e a comunicação oral dos estudantes.",
            InformacoesAdicionais = "Traje: uniforme escolar. Chegada com 20 minutos de antecedência.",
            DataInicio = DateTime.Today.AddDays(7).AddHours(13),
            DataFim = DateTime.Today.AddDays(7).AddHours(18),
            Local = "Auditório Principal",
            CargaHorariaHoras = 5,
            LimiteVagas = 80,
            Status = StatusEvento.Publicado,
            CriadoPorUsuarioId = administrador.Id,
            DataCriacao = DateTime.UtcNow
        };

        var sarau = new Evento
        {
            Titulo = "Sarau Literário",
            Subtitulo = "Poesia, música e leitura compartilhada",
            Descricao = "Noite cultural com recitais de poesia, apresentações musicais e um espaço de leitura aberta para a comunidade escolar.",
            Objetivo = "Valorizar a produção literária dos alunos e fortalecer o convívio cultural da escola.",
            InformacoesAdicionais = "Entrada franca. Leve um texto curto se quiser participar da leitura aberta.",
            DataInicio = DateTime.Today.AddDays(14).AddHours(19),
            DataFim = DateTime.Today.AddDays(14).AddHours(21),
            Local = "Pátio coberto",
            CargaHorariaHoras = 2,
            LimiteVagas = 120,
            Status = StatusEvento.Publicado,
            CriadoPorUsuarioId = administrador.Id,
            DataCriacao = DateTime.UtcNow
        };

        contexto.Eventos.AddRange(feira, sarau);
        await contexto.SaveChangesAsync();

        contexto.ProfessoresAutorizadosEvento.AddRange(
            new ProfessorAutorizadoEvento
            {
                EventoId = feira.Id,
                ProfessorId = professor.Id,
                AutorizadoPorUsuarioId = administrador.Id,
                DataAutorizacao = DateTime.UtcNow,
                PodeEditarEvento = true,
                PodeAcessarPresenca = true
            },
            new ProfessorAutorizadoEvento
            {
                EventoId = sarau.Id,
                ProfessorId = professorAuxiliar.Id,
                AutorizadoPorUsuarioId = administrador.Id,
                DataAutorizacao = DateTime.UtcNow,
                PodeEditarEvento = true,
                PodeAcessarPresenca = true
            });

        contexto.Inscricoes.Add(new Inscricao
        {
            EventoId = feira.Id,
            AlunoId = aluno.Id,
            DataInscricao = DateTime.UtcNow,
            Status = StatusInscricao.Ativa
        });

        await contexto.SaveChangesAsync();
    }

    private static async Task ComplementarEventoExistenteAsync(
        ApplicationDbContext contexto,
        Usuario professor,
        Usuario professorAuxiliar)
    {
        var evento = await contexto.Eventos.OrderBy(item => item.Id).FirstOrDefaultAsync();
        if (evento is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(evento.Subtitulo))
        {
            evento.Subtitulo = "Projetos do ensino médio em exposição";
            evento.Objetivo = "Estimular a investigação científica e a comunicação oral dos estudantes.";
            evento.InformacoesAdicionais = "Traje: uniforme escolar. Chegada com 20 minutos de antecedência.";
        }

        var vinculo = await contexto.ProfessoresAutorizadosEvento
            .FirstOrDefaultAsync(item => item.EventoId == evento.Id && item.ProfessorId == professor.Id);

        if (vinculo is not null && !vinculo.PodeEditarEvento && !vinculo.PodeAcessarPresenca)
        {
            vinculo.PodeEditarEvento = true;
            vinculo.PodeAcessarPresenca = true;
        }

        if (!await contexto.Eventos.AnyAsync(item => item.Titulo == "Sarau Literário"))
        {
            var sarau = new Evento
            {
                Titulo = "Sarau Literário",
                Subtitulo = "Poesia, música e leitura compartilhada",
                Descricao = "Noite cultural com recitais de poesia, apresentações musicais e um espaço de leitura aberta para a comunidade escolar.",
                Objetivo = "Valorizar a produção literária dos alunos e fortalecer o convívio cultural da escola.",
                InformacoesAdicionais = "Entrada franca. Leve um texto curto se quiser participar da leitura aberta.",
                DataInicio = DateTime.Today.AddDays(14).AddHours(19),
                DataFim = DateTime.Today.AddDays(14).AddHours(21),
                Local = "Pátio coberto",
                CargaHorariaHoras = 2,
                LimiteVagas = 120,
                Status = StatusEvento.Publicado,
                CriadoPorUsuarioId = evento.CriadoPorUsuarioId,
                DataCriacao = DateTime.UtcNow
            };

            contexto.Eventos.Add(sarau);
            await contexto.SaveChangesAsync();

            contexto.ProfessoresAutorizadosEvento.Add(new ProfessorAutorizadoEvento
            {
                EventoId = sarau.Id,
                ProfessorId = professorAuxiliar.Id,
                AutorizadoPorUsuarioId = evento.CriadoPorUsuarioId,
                DataAutorizacao = DateTime.UtcNow,
                PodeEditarEvento = true,
                PodeAcessarPresenca = true
            });
        }

        await contexto.SaveChangesAsync();
    }
}
