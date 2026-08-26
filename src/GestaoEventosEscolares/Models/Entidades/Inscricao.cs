using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.Entidades;

public class Inscricao
{
    public int Id { get; set; }

    public int EventoId { get; set; }

    public Evento Evento { get; set; } = null!;

    public string AlunoId { get; set; } = string.Empty;

    public Usuario Aluno { get; set; } = null!;

    public DateTime DataInscricao { get; set; } = DateTime.UtcNow;

    public StatusInscricao Status { get; set; } = StatusInscricao.Ativa;

    /// <summary>
    /// Identificador único da inscrição (aluno + evento), embutido no QR Code.
    /// </summary>
    public string CodigoQr { get; set; } = string.Empty;

    public Presenca? Presenca { get; set; }

    public Certificado? Certificado { get; set; }
}
