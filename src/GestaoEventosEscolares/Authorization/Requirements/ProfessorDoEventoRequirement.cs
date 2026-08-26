using GestaoEventosEscolares.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace GestaoEventosEscolares.Authorization.Requirements;

public class ProfessorDoEventoRequirement : IAuthorizationRequirement
{
    public ProfessorDoEventoRequirement(PermissaoEvento permissao = PermissaoEvento.QualquerVinculo)
    {
        Permissao = permissao;
    }

    public PermissaoEvento Permissao { get; }
}
