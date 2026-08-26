namespace GestaoEventosEscolares.Services;

/// <summary>
/// Payload do QR: GEE:{eventoId}:{codigoUnico}. O prefixo evita colisão e permite detectar evento errado.
/// </summary>
public static class PayloadQrInscricao
{
    public const string Prefixo = "GEE";

    public static string Montar(int eventoId, string codigoQr)
        => $"{Prefixo}:{eventoId}:{codigoQr}";

    public static string GerarCodigo()
        => Guid.NewGuid().ToString("N");

    public static bool TentarLer(string bruto, out int? eventoIdNoQr, out string codigoQr)
    {
        eventoIdNoQr = null;
        codigoQr = string.Empty;
        var texto = bruto.Trim();

        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        var partes = texto.Split(':', StringSplitOptions.TrimEntries);
        if (partes.Length == 3
            && partes[0].Equals(Prefixo, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(partes[1], out var eventoId)
            && partes[2].Length > 0)
        {
            eventoIdNoQr = eventoId;
            codigoQr = partes[2];
            return true;
        }

        if (partes.Length == 1 && texto.Length is >= 16 and <= 64)
        {
            codigoQr = texto;
            return true;
        }

        return false;
    }
}
