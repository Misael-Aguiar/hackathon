using GestaoEventosEscolares.Models.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services;

/// <summary>
/// Inclui RM e nome completo no cookie de autenticação para evitar consultas extras nas views.
/// </summary>
public class UsuarioClaimsPrincipalFactory : UserClaimsPrincipalFactory<Usuario, IdentityRole>
{
    public UsuarioClaimsPrincipalFactory(
        UserManager<Usuario> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimTypesPersonalizados.RM, user.RM));
        identity.AddClaim(new Claim(ClaimTypesPersonalizados.NomeCompleto, user.NomeCompleto));
        identity.AddClaim(new Claim(ClaimTypesPersonalizados.Perfil, user.Perfil.ToString()));
        return identity;
    }
}

public static class ClaimTypesPersonalizados
{
    public const string RM = "rm";
    public const string NomeCompleto = "nome_completo";
    public const string Perfil = "perfil";
}
