using GestaoEventosEscolares.Models.Entidades;

namespace GestaoEventosEscolares.Data.Consultas;

/// <summary>
/// Regras de ciclo de vida aplicadas em IQueryable (SQL), sem BackgroundService.
/// Um hosted service só faria sentido para persistir Status=Encerrado em lote;
/// para inscrição e listagem, a data do evento já é a fonte da verdade.
/// </summary>
public static class EventoConsultas
{
    public const int DiasExibicaoAposEvento = 7;

    /// <summary>
    /// Eventos com DataInicio anterior a este instante saem da listagem de aluno/professor.
    /// </summary>
    public static DateTime LimiteExibicaoListagem(DateTime agora)
        => agora.AddDays(-DiasExibicaoAposEvento);

    /// <summary>
    /// Inscrição fecha no instante DataInicio — não depois, nem no dia seguinte.
    /// </summary>
    public static bool InscricaoAberta(DateTime dataInicioEvento, DateTime agora)
        => agora < dataInicioEvento;

    /// <summary>
    /// Filtro de vitrine: 1 semana após a data do evento. Admin não usa este método.
    /// </summary>
    public static IQueryable<Evento> OndeDentroDaJanelaDeExibicao(
        this IQueryable<Evento> consulta,
        DateTime agora)
    {
        var limite = LimiteExibicaoListagem(agora);
        return consulta.Where(evento => evento.DataInicio >= limite);
    }

    /// <summary>
    /// Listagem: data mais recente primeiro.
    /// </summary>
    public static IQueryable<Evento> OrdenarPorDataRecente(this IQueryable<Evento> consulta)
        => consulta
            .OrderByDescending(evento => evento.DataInicio)
            .ThenBy(evento => evento.Titulo);
}
