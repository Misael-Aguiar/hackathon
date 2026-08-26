namespace GestaoEventosEscolares.Models.Entidades;

/// <summary>
/// Registro de presença do aluno. ValidadoPorUsuarioId aponta para o professor (ou admin) que confirmou.
/// </summary>
public class Presenca
{
    public int Id { get; set; }

    public int EventoId { get; set; }

    public Evento Evento { get; set; } = null!;

    public int InscricaoId { get; set; }

    public Inscricao Inscricao { get; set; } = null!;

    public string AlunoId { get; set; } = string.Empty;

    public Usuario Aluno { get; set; } = null!;

    public string ValidadoPorUsuarioId { get; set; } = string.Empty;

    public Usuario ValidadoPor { get; set; } = null!;

    public DateTime DataValidacao { get; set; } = DateTime.UtcNow;

    public string? CodigoQrUtilizado { get; set; }
}
