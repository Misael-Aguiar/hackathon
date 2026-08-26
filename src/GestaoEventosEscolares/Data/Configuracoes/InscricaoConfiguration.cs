using GestaoEventosEscolares.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoEventosEscolares.Data.Configuracoes;

public class InscricaoConfiguration : IEntityTypeConfiguration<Inscricao>
{
    public void Configure(EntityTypeBuilder<Inscricao> builder)
    {
        builder.ToTable("Inscricoes");

        builder.HasKey(inscricao => inscricao.Id);

        builder.HasIndex(inscricao => new { inscricao.EventoId, inscricao.AlunoId })
            .IsUnique();

        builder.Property(inscricao => inscricao.CodigoQr)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(inscricao => inscricao.CodigoQr)
            .IsUnique();

        builder.HasOne(inscricao => inscricao.Evento)
            .WithMany(evento => evento.Inscricoes)
            .HasForeignKey(inscricao => inscricao.EventoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inscricao => inscricao.Aluno)
            .WithMany(usuario => usuario.Inscricoes)
            .HasForeignKey(inscricao => inscricao.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
