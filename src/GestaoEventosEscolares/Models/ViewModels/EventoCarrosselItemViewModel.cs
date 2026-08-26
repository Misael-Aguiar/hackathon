using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.ViewModels;

public class EventoCarrosselItemViewModel
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Subtitulo { get; set; } = string.Empty;

    public string? CaminhoImagem { get; set; }

    public DateTime DataInicio { get; set; }

    public string DataResumida => DataInicio.ToString("dd MMM yyyy", new System.Globalization.CultureInfo("pt-BR"));
}
