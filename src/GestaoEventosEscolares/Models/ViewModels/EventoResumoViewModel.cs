using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.ViewModels;

public class EventoResumoViewModel
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Subtitulo { get; set; } = string.Empty;

    public string? CaminhoImagem { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime DataFim { get; set; }

    public string Local { get; set; } = string.Empty;

    public StatusEvento Status { get; set; }

    public int TotalInscritos { get; set; }

    public bool PodeEditar { get; set; }

    public bool PodeGerenciarPermissoes { get; set; }

    public bool PodeValidarPresenca { get; set; }
}
