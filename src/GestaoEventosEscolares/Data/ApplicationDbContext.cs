using GestaoEventosEscolares.Models.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Evento> Eventos => Set<Evento>();

    public DbSet<Inscricao> Inscricoes => Set<Inscricao>();

    public DbSet<Presenca> Presencas => Set<Presenca>();

    public DbSet<Certificado> Certificados => Set<Certificado>();

    public DbSet<ProfessorAutorizadoEvento> ProfessoresAutorizadosEvento => Set<ProfessorAutorizadoEvento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Identity: tabelas com nomes em português, alinhados ao restante do schema.
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        modelBuilder.Entity<IdentityRole>().ToTable("Perfis");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UsuariosPerfis");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UsuariosClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UsuariosLogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("PerfisClaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UsuariosTokens");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
