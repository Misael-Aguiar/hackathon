using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Authorization;

public static class PoliticasAutorizacao
{
    public const string SomenteAdministrador = "SomenteAdministrador";
    public const string SomenteProfessor = "SomenteProfessor";
    public const string ProfessorOuAdministrador = "ProfessorOuAdministrador";
    public const string SomenteAluno = "SomenteAluno";

    /// <summary>
    /// Administrador passa sempre. Professor só passa se estiver vinculado ao evento da rota.
    /// </summary>
    public const string ProfessorDoEvento = "ProfessorDoEvento";

    public const string ProfessorPodeEditarEvento = "ProfessorPodeEditarEvento";

    public const string ProfessorPodeAcessarPresenca = "ProfessorPodeAcessarPresenca";

    public static string NomePerfil(PerfilUsuario perfil) => perfil.ToString();
}
