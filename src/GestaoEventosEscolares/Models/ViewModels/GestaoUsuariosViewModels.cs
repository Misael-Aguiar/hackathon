using System.ComponentModel.DataAnnotations;
using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.ViewModels;

/// <summary>
/// Formulário da aba Alunos: nome, RM e sala (DS1 / DS2 / DS3).
/// </summary>
public class CadastroAlunoViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [Display(Name = "Nome")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o RM.")]
    [Display(Name = "RM")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "O RM deve ter entre 3 e 20 caracteres.")]
    [RegularExpression(@"^[0-9A-Za-z]+$", ErrorMessage = "O RM deve conter apenas letras e números.")]
    public string RM { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione a sala.")]
    [Display(Name = "Sala")]
    public SalaTurma? Sala { get; set; }
}

public class EditarAlunoViewModel : CadastroAlunoViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Formulário da aba Professores: nome, RM e telefone.
/// </summary>
public class CadastroProfessorViewModel
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [Display(Name = "Nome")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o RM.")]
    [Display(Name = "RM")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "O RM deve ter entre 3 e 20 caracteres.")]
    [RegularExpression(@"^[0-9A-Za-z]+$", ErrorMessage = "O RM deve conter apenas letras e números.")]
    public string RM { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone.")]
    [Display(Name = "Telefone")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "Informe um telefone válido.")]
    [RegularExpression(@"^[0-9()\s\-+]+$", ErrorMessage = "Use apenas números e caracteres de telefone.")]
    public string Telefone { get; set; } = string.Empty;
}

public class AlunoListagemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public SalaTurma Sala { get; set; }

    public int TotalInscricoesAtivas { get; set; }
}

public class ProfessorListagemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public int TotalEventosVinculados { get; set; }
}

public class GestaoAlunosViewModel
{
    public CadastroAlunoViewModel Cadastro { get; set; } = new();

    public IReadOnlyList<AlunoListagemViewModel> Alunos { get; set; } = [];
}

public class GestaoProfessoresViewModel
{
    public CadastroProfessorViewModel Cadastro { get; set; } = new();

    public IReadOnlyList<ProfessorListagemViewModel> Professores { get; set; } = [];
}

public class ResultadoCadastroUsuario
{
    public string Nome { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public string SenhaInicial { get; set; } = string.Empty;
}
