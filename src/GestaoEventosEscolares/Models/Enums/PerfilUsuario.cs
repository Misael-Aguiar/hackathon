namespace GestaoEventosEscolares.Models.Enums;

/// <summary>
/// Perfil de acesso do usuário. Espelha as roles do Identity para consultas no domínio.
/// </summary>
public enum PerfilUsuario
{
    Aluno = 1,
    Professor = 2,
    Administrador = 3
}
