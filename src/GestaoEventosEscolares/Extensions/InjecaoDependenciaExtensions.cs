using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Authorization.Handlers;
using GestaoEventosEscolares.Authorization.Requirements;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Services;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Extensions;

public static class InjecaoDependenciaExtensions
{
    public static IServiceCollection AdicionarCamadaDeDados(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    public static IServiceCollection AdicionarAutenticacaoEAutorizacao(this IServiceCollection services)
    {
        services.AddIdentity<Usuario, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AcessoNegado";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.Cookie.Name = "GestaoEventos.Auth";
        });

        services.AddScoped<IUserClaimsPrincipalFactory<Usuario>, UsuarioClaimsPrincipalFactory>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PoliticasAutorizacao.SomenteAdministrador, policy =>
                policy.RequireRole(NomesPerfis.Administrador));

            options.AddPolicy(PoliticasAutorizacao.SomenteProfessor, policy =>
                policy.RequireRole(NomesPerfis.Professor));

            options.AddPolicy(PoliticasAutorizacao.ProfessorOuAdministrador, policy =>
                policy.RequireRole(NomesPerfis.Professor, NomesPerfis.Administrador));

            options.AddPolicy(PoliticasAutorizacao.SomenteAluno, policy =>
                policy.RequireRole(NomesPerfis.Aluno));

            options.AddPolicy(PoliticasAutorizacao.ProfessorDoEvento, policy =>
                policy.AddRequirements(new ProfessorDoEventoRequirement(PermissaoEvento.QualquerVinculo)));

            options.AddPolicy(PoliticasAutorizacao.ProfessorPodeEditarEvento, policy =>
                policy.AddRequirements(new ProfessorDoEventoRequirement(PermissaoEvento.Editar)));

            options.AddPolicy(PoliticasAutorizacao.ProfessorPodeAcessarPresenca, policy =>
                policy.AddRequirements(new ProfessorDoEventoRequirement(PermissaoEvento.AcessarPresenca)));
        });

        services.AddScoped<IAuthorizationHandler, ProfessorDoEventoAuthorizationHandler>();
        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AdicionarServicosDeAplicacao(this IServiceCollection services)
    {
        services.AddScoped<IAutorizacaoEventoService, AutorizacaoEventoService>();
        services.AddScoped<IConsultaEventoService, ConsultaEventoService>();
        services.AddScoped<IGestaoEventoService, GestaoEventoService>();
        services.AddScoped<IArmazenamentoImagemEventoService, ArmazenamentoImagemEventoService>();
        services.AddScoped<IInscricaoService, InscricaoService>();
        services.AddScoped<IPresencaService, PresencaService>();
        services.AddScoped<IGeradorQrCodeService, GeradorQrCodeService>();
        return services;
    }
}
