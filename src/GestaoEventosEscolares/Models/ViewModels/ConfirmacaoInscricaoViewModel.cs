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
}
