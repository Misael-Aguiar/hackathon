namespace GestaoEventosEscolares.Services;

/// <summary>
/// Payload do QR: GEE:{eventoId}:{guid32}. O check-in também aceita o apelido curto da inscrição.
/// </summary>
public static class PayloadQrInscricao
{
    public const string Prefixo = "GEE";

    public static string Montar(int eventoId, string codigoQr)
        => $"{Prefixo}:{eventoId}:{codigoQr}";

    /// <summary>GUID sem hífen — não sequencial, 128 bits de entropia.</summary>
    public static string GerarCodigo()
        => Guid.NewGuid().ToString("N");

    public static bool TentarLer(string bruto, out int? eventoIdNoQr, out string codigoQr)
    {
        eventoIdNoQr = null;
        codigoQr = string.Empty;
        if (string.IsNullOrWhiteSpace(bruto))
        {
            return false;
        }

        var partes = bruto.Trim().Split(':', StringSplitOptions.TrimEntries);
        // Recusa payload sem prefixo (GUID solto ou Id sequencial).
        if (partes.Length != 3
            || !partes[0].Equals(Prefixo, StringComparison.OrdinalIgnoreCase)
            || !int.TryParse(partes[1], out var eventoId)
            || !Guid.TryParseExact(partes[2], "N", out _))
        {
            return false;
        }

        eventoIdNoQr = eventoId;
        codigoQr = partes[2];
        return true;
    }
}
