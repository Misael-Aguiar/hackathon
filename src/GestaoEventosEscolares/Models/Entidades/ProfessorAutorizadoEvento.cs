namespace GestaoEventosEscolares.Models.Entidades;

/// <summary>
/// Vínculo que autoriza um professor a validar presença e editar um evento específico.
/// </summary>
public class ProfessorAutorizadoEvento
{
    public int Id { get; set; }

    public int EventoId { get; set; }

    public Evento Evento { get; set; } = null!;

    public string ProfessorId { get; set; } = string.Empty;

    public Usuario Professor { get; set; } = null!;

    public string AutorizadoPorUsuarioId { get; set; } = string.Empty;

    public Usuario AutorizadoPor { get; set; } = null!;

    public DateTime DataAutorizacao { get; set; } = DateTime.UtcNow;

    /// <summary>Permite ao professor acessar a tabela de presença do evento.</summary>
    public bool PodeAcessarPresenca { get; set; }

    /// <summary>Permite ao professor editar os dados do evento.</summary>
    public bool PodeEditarEvento { get; set; }
}
