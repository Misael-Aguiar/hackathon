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
    /// Token imprevisível (GUID N, 32 hex). Nunca use o Id sequencial da inscrição no QR.
    /// Rotacionado no cancelamento para invalidar o PNG já emitido.
    /// </summary>
    public string CodigoQr { get; set; } = string.Empty;

    /// <summary>
    /// Apelido de 8 caracteres para digitação. Aponta para a mesma inscrição que CodigoQr.
    /// Rotacionado junto com o GUID no cancelamento.
    /// </summary>
    public string CodigoCheckIn { get; set; } = string.Empty;

    public Presenca? Presenca { get; set; }

    public Certificado? Certificado { get; set; }
}
