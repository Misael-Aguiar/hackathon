using System.Globalization;

namespace GestaoEventosEscolares.Extensions;

public static class FormatacaoAgenda
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static string ParaDataHora(this DateTime data)
        => data.ToString("dd/MM/yyyy HH:mm", PtBr);

    public static string ParaData(this DateTime data)
        => data.ToString("dd/MM/yyyy", PtBr);

    public static string ParaHora(this DateTime data)
        => data.ToString("HH:mm", PtBr);
}
