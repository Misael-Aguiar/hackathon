using System.ComponentModel.DataAnnotations;

namespace GestaoEventosEscolares.Models.ViewModels;

public class PermissoesEventoViewModel
{
    public int EventoId { get; set; }

    public string TituloEvento { get; set; } = string.Empty;

    public List<PermissaoProfessorItemViewModel> Professores { get; set; } = [];
}

public class PermissaoProfessorItemViewModel
{
    public string ProfessorId { get; set; } = string.Empty;

    public string NomeCompleto { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    [Display(Name = "Acessar tabela de presença")]
    public bool PodeAcessarPresenca { get; set; }

    [Display(Name = "Editar o evento")]
    public bool PodeEditarEvento { get; set; }
}
