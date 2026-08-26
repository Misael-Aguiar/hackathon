using GestaoEventosEscolares.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoEventosEscolares.Data.Configuracoes;

public class CertificadoConfiguration : IEntityTypeConfiguration<Certificado>
{
    public void Configure(EntityTypeBuilder<Certificado> builder)
    {
        builder.ToTable("Certificados");

        builder.HasKey(certificado => certificado.Id);

        builder.HasIndex(certificado => certificado.InscricaoId)
            .IsUnique();

        builder.Property(certificado => certificado.CodigoVerificacao)
            .IsRequired()
            .HasMaxLength(40);

        builder.HasIndex(certificado => certificado.CodigoVerificacao)
            .IsUnique();

        builder.Property(certificado => certificado.CaminhoArquivo)
            .HasMaxLength(500);

        builder.HasOne(certificado => certificado.Evento)
            .WithMany(evento => evento.Certificados)
            .HasForeignKey(certificado => certificado.EventoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(certificado => certificado.Inscricao)
            .WithOne(inscricao => inscricao.Certificado)
            .HasForeignKey<Certificado>(certificado => certificado.InscricaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(certificado => certificado.Aluno)
            .WithMany(usuario => usuario.Certificados)
            .HasForeignKey(certificado => certificado.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
