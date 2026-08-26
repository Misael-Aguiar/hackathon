using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Extensions;

public static class EnumTextoExtensions
{
    public static string ParaTexto(this StatusEvento status) => status switch
    {
        StatusEvento.Rascunho => "Rascunho",
        StatusEvento.Publicado => "Publicado",
        StatusEvento.EmAndamento => "Em andamento",
        StatusEvento.Encerrado => "Encerrado",
        StatusEvento.Cancelado => "Cancelado",
        _ => status.ToString()
    };

    public static string ParaClasseCss(this StatusEvento status) => status switch
    {
        StatusEvento.Publicado => "badge-status badge-status--publicado",
        StatusEvento.EmAndamento => "badge-status badge-status--andamento",
        StatusEvento.Encerrado => "badge-status badge-status--encerrado",
        StatusEvento.Cancelado => "badge-status badge-status--cancelado",
        _ => "badge-status badge-status--rascunho"
    };

    public static string ParaTexto(this PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Administrador => "Administrador",
        PerfilUsuario.Professor => "Professor",
        PerfilUsuario.Aluno => "Aluno",
        _ => perfil.ToString()
    };
}
