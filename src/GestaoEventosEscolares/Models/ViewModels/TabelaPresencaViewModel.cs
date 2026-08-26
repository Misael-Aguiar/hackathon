namespace GestaoEventosEscolares.Models.ViewModels;

public class TabelaPresencaViewModel
{
    public int EventoId { get; set; }

    public string TituloEvento { get; set; } = string.Empty;

    public int TotalInscritos { get; set; }

    public int TotalPresentes { get; set; }

    public IReadOnlyList<AlunoPresenteViewModel> Presentes { get; set; } = [];

    public IReadOnlyList<AlunoPendenteViewModel> Pendentes { get; set; } = [];
}

public class AlunoPresenteViewModel
{
    public string NomeCompleto { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public DateTime DataValidacao { get; set; }

    public string ValidadoPor { get; set; } = string.Empty;

    public string ValidadoPorRm { get; set; } = string.Empty;
}

public class AlunoPendenteViewModel
{
    public string NomeCompleto { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;
}

public class ValidarPresencaPaginaViewModel
{
    public int EventoId { get; set; }

    public string TituloEvento { get; set; } = string.Empty;
}
