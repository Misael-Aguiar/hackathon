using GestaoEventosEscolares.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoEventosEscolares.Data.Configuracoes;

public class ProfessorAutorizadoEventoConfiguration : IEntityTypeConfiguration<ProfessorAutorizadoEvento>
{
    public void Configure(EntityTypeBuilder<ProfessorAutorizadoEvento> builder)
    {
        builder.ToTable("ProfessoresAutorizadosEvento");

        builder.HasKey(vinculo => vinculo.Id);

        builder.HasIndex(vinculo => new { vinculo.EventoId, vinculo.ProfessorId })
            .IsUnique();

        builder.HasOne(vinculo => vinculo.Evento)
            .WithMany(evento => evento.ProfessoresAutorizados)
            .HasForeignKey(vinculo => vinculo.EventoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vinculo => vinculo.Professor)
            .WithMany(usuario => usuario.EventosAutorizados)
            .HasForeignKey(vinculo => vinculo.ProfessorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(vinculo => vinculo.AutorizadoPor)
            .WithMany()
            .HasForeignKey(vinculo => vinculo.AutorizadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
