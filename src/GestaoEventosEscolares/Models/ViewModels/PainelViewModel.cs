using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.ViewModels;

public class PainelViewModel
{
    public string NomeCompleto { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public PerfilUsuario Perfil { get; set; }

    public int TotalEventosVisiveis { get; set; }

    public IReadOnlyList<EventoResumoViewModel> EventosDestaque { get; set; } = [];
}
