namespace GestaoEventosEscolares.Models.Enums;

/// <summary>
/// Granularidade do vínculo professor-evento usada nas policies de autorização.
/// </summary>
public enum PermissaoEvento
{
    QualquerVinculo = 0,
    Editar = 1,
    AcessarPresenca = 2
}
