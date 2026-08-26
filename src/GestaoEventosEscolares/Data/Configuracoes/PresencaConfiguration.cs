using GestaoEventosEscolares.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoEventosEscolares.Data.Configuracoes;

public class PresencaConfiguration : IEntityTypeConfiguration<Presenca>
{
    public void Configure(EntityTypeBuilder<Presenca> builder)
    {
        builder.ToTable("Presencas");

        builder.HasKey(presenca => presenca.Id);

        builder.HasIndex(presenca => presenca.InscricaoId)
            .IsUnique();

        builder.Property(presenca => presenca.CodigoQrUtilizado)
            .HasMaxLength(100);

        builder.HasOne(presenca => presenca.Evento)
            .WithMany(evento => evento.Presencas)
            .HasForeignKey(presenca => presenca.EventoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(presenca => presenca.Inscricao)
            .WithOne(inscricao => inscricao.Presenca)
            .HasForeignKey<Presenca>(presenca => presenca.InscricaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(presenca => presenca.Aluno)
            .WithMany(usuario => usuario.PresencasComoParticipante)
            .HasForeignKey(presenca => presenca.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict evita ciclo de cascade com AlunoId apontando para a mesma tabela Usuarios.
        builder.HasOne(presenca => presenca.ValidadoPor)
            .WithMany(usuario => usuario.PresencasValidadas)
            .HasForeignKey(presenca => presenca.ValidadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
