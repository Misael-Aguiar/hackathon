using GestaoEventosEscolares.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoEventosEscolares.Data.Configuracoes;

public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("Eventos");

        builder.HasKey(evento => evento.Id);

        builder.Property(evento => evento.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(evento => evento.Subtitulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(evento => evento.Descricao)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(evento => evento.Objetivo)
            .IsRequired()
            .HasMaxLength(1500);

        builder.Property(evento => evento.InformacoesAdicionais)
            .HasMaxLength(2000);

        builder.Property(evento => evento.CaminhoImagem)
            .HasMaxLength(300);

        builder.Property(evento => evento.Local)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(evento => evento.CriadoPor)
            .WithMany(usuario => usuario.EventosCriados)
            .HasForeignKey(evento => evento.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
