namespace GestaoEventosEscolares.Models.Entidades;

public class Certificado
{
    public int Id { get; set; }

    public int EventoId { get; set; }

    public Evento Evento { get; set; } = null!;

    public int InscricaoId { get; set; }

    public Inscricao Inscricao { get; set; } = null!;

    public string AlunoId { get; set; } = string.Empty;

    public Usuario Aluno { get; set; } = null!;

    public string CodigoVerificacao { get; set; } = string.Empty;

    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;

    public string? CaminhoArquivo { get; set; }
}
