using System.ComponentModel.DataAnnotations;
using GestaoEventosEscolares.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestaoEventosEscolares.Models.ViewModels;

public class FormularioEventoViewModel
{
    public int Id { get; set; }

    public bool EhEdicao => Id > 0;

    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(200)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o subtítulo.")]
    [StringLength(200)]
    [Display(Name = "Subtítulo")]
    public string Subtitulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descrição.")]
    [StringLength(4000)]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o objetivo.")]
    [StringLength(1500)]
    [Display(Name = "Objetivo do evento")]
    public string Objetivo { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Informações adicionais")]
    public string? InformacoesAdicionais { get; set; }

    [Required(ErrorMessage = "Informe o local.")]
    [StringLength(200)]
    [Display(Name = "Local")]
    public string Local { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data")]
    public DateOnly Data { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [Required(ErrorMessage = "Informe o horário de início.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de início")]
    public TimeOnly HoraInicio { get; set; } = new(8, 0);

    [Required(ErrorMessage = "Informe o horário de término.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de término")]
    public TimeOnly HoraFim { get; set; } = new(12, 0);

    [Display(Name = "Status")]
    public StatusEvento Status { get; set; } = StatusEvento.Publicado;

    [Display(Name = "Imagem do evento")]
    public IFormFile? Imagem { get; set; }

    public string? CaminhoImagemAtual { get; set; }

    [Display(Name = "Professores responsáveis")]
    public List<string> ProfessoresResponsaveisIds { get; set; } = [];

    public IReadOnlyList<SelectListItem> ProfessoresDisponiveis { get; set; } = [];

    public bool PodeAlterarPermissoes { get; set; }
}
