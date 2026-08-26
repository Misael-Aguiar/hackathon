using GestaoEventosEscolares.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace GestaoEventosEscolares.Models.Entidades;

/// <summary>
/// Usuário do sistema. O login é feito pelo RM (matrícula), mapeado também como UserName do Identity.
/// </summary>
public class Usuario : IdentityUser
{
    public string RM { get; set; } = string.Empty;

    public string NomeCompleto { get; set; } = string.Empty;

    public PerfilUsuario Perfil { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();

    public ICollection<Presenca> PresencasComoParticipante { get; set; } = new List<Presenca>();

    public ICollection<Presenca> PresencasValidadas { get; set; } = new List<Presenca>();

    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();

    public ICollection<ProfessorAutorizadoEvento> EventosAutorizados { get; set; } = new List<ProfessorAutorizadoEvento>();

    public ICollection<Evento> EventosCriados { get; set; } = new List<Evento>();
}
