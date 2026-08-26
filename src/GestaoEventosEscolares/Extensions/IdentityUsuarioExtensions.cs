using System.Security.Claims;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Services;

namespace GestaoEventosEscolares.Extensions;

public static class IdentityUsuarioExtensions
{
    public static string? ObterId(this ClaimsPrincipal usuario)
        => usuario.FindFirstValue(ClaimTypes.NameIdentifier);

    public static string ObterRm(this ClaimsPrincipal usuario)
        => usuario.FindFirstValue(ClaimTypesPersonalizados.RM)
           ?? usuario.Identity?.Name
           ?? string.Empty;

    public static string ObterNomeCompleto(this ClaimsPrincipal usuario)
        => usuario.FindFirstValue(ClaimTypesPersonalizados.NomeCompleto)
           ?? usuario.Identity?.Name
           ?? "Usuário";

    public static PerfilUsuario ObterPerfil(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypesPersonalizados.Perfil);
        return Enum.TryParse<PerfilUsuario>(valor, out var perfil)
            ? perfil
            : InferirPerfilPorRole(usuario);
    }

    public static bool EhAdministrador(this ClaimsPrincipal usuario)
        => usuario.IsInRole(NomesPerfis.Administrador);

    public static bool EhProfessor(this ClaimsPrincipal usuario)
        => usuario.IsInRole(NomesPerfis.Professor);

    public static bool EhAluno(this ClaimsPrincipal usuario)
        => usuario.IsInRole(NomesPerfis.Aluno);

    private static PerfilUsuario InferirPerfilPorRole(ClaimsPrincipal usuario)
    {
        if (usuario.EhAdministrador())
        {
            return PerfilUsuario.Administrador;
        }

        if (usuario.EhProfessor())
        {
            return PerfilUsuario.Professor;
        }

        return PerfilUsuario.Aluno;
    }
}
