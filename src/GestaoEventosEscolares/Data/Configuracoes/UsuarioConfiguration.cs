using GestaoEventosEscolares.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoEventosEscolares.Data.Configuracoes;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.Property(usuario => usuario.RM)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(usuario => usuario.RM)
            .IsUnique();

        builder.Property(usuario => usuario.NomeCompleto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(usuario => usuario.Perfil)
            .IsRequired();

        builder.Property(usuario => usuario.Sala)
            .IsRequired(false);

        builder.Property(usuario => usuario.Telefone)
            .HasMaxLength(20);

        builder.Property(usuario => usuario.Ativo)
            .HasDefaultValue(true);
    }
}
