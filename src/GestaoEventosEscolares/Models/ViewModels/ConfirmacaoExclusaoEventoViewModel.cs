namespace GestaoEventosEscolares.Models.ViewModels;

/// <summary>
/// Tela de impacto antes da exclusão física do evento (somente admin).
/// </summary>
public class ConfirmacaoExclusaoEventoViewModel
{
    public int EventoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public int TotalInscricoes { get; set; }

    public int TotalPresencas { get; set; }

    public int TotalCertificados { get; set; }

    public bool TemHistorico => TotalPresencas > 0 || TotalCertificados > 0;
}
