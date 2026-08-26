namespace GestaoEventosEscolares.Models.ViewModels;

public class ConfirmacaoInscricaoViewModel
{
    public int InscricaoId { get; set; }

    public int EventoId { get; set; }

    public string TituloEvento { get; set; } = string.Empty;

    public string NomeAluno { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }

    public string Local { get; set; } = string.Empty;

    public string PayloadQr { get; set; } = string.Empty;

    public bool PresencaConfirmada { get; set; }

    public bool PodeBaixarCertificado { get; set; }

    public bool CertificadoEmitido { get; set; }

    public bool QrDisponivel { get; set; }

    public bool PodeCancelar { get; set; }

    /// <summary>Apelido digitável da mesma inscrição do QR (não é um recorte do GUID).</summary>
    public string CodigoCheckIn { get; set; } = string.Empty;

    public string CodigoCheckInFormatado =>
        CodigoCheckIn.Length == 8
            ? $"{CodigoCheckIn[..4]}-{CodigoCheckIn[4..]}"
            : CodigoCheckIn;
}
