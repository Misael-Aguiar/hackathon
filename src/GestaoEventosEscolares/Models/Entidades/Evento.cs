using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.Entidades;

public class Evento
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }

    public DateTime DataFim { get; set; }

    public string Local { get; set; } = string.Empty;

    public int CargaHorariaHoras { get; set; }

    public int? LimiteVagas { get; set; }

    public StatusEvento Status { get; set; } = StatusEvento.Rascunho;

    public string CriadoPorUsuarioId { get; set; } = string.Empty;

    public Usuario CriadoPor { get; set; } = null!;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();

    public ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();

    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();

    public ICollection<ProfessorAutorizadoEvento> ProfessoresAutorizados { get; set; } = new List<ProfessorAutorizadoEvento>();
}
