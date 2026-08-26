using System.ComponentModel.DataAnnotations;

namespace GestaoEventosEscolares.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o RM.")]
    [Display(Name = "RM (matrícula)")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "O RM deve ter entre 3 e 20 caracteres.")]
    public string RM { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Lembrar-me")]
    public bool LembrarMe { get; set; }
}
