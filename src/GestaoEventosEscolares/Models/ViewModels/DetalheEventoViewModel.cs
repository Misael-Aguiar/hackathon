using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.ViewModels;

public class DetalheEventoViewModel
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Subtitulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Objetivo { get; set; } = string.Empty;

    public string? InformacoesAdicionais { get; set; }

    public string? CaminhoImagem { get; set; }

    public string Local { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }

    public DateTime DataFim { get; set; }

    public StatusEvento Status { get; set; }

    public IReadOnlyList<string> ProfessoresResponsaveis { get; set; } = [];

    public bool PodeEditar { get; set; }

    public bool PodeGerenciarPermissoes { get; set; }

    public bool PodeValidarPresenca { get; set; }

    public bool PodeInscrever { get; set; }

    public bool JaInscrito { get; set; }

    public int? InscricaoId { get; set; }

    public bool PrecisaLoginAluno { get; set; }

    public string? MotivoBloqueioInscricao { get; set; }
}
