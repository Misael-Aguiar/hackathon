using System.Security.Cryptography;

namespace GestaoEventosEscolares.Services;

/// <summary>
/// Apelido curto da inscrição. Resolve para o mesmo registro que o GUID do QR.
/// Sem 0/O/1/I/L para reduzir erro na digitação.
/// </summary>
public static class GeradorCodigoCheckIn
{
    public const int Tamanho = 8;

    private const string Alfabeto = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    public static string Gerar()
    {
        Span<char> buffer = stackalloc char[Tamanho];
        for (var i = 0; i < Tamanho; i++)
        {
            buffer[i] = Alfabeto[RandomNumberGenerator.GetInt32(Alfabeto.Length)];
        }

        return new string(buffer);
    }

    public static bool TentarNormalizar(string? bruto, out string codigo)
    {
        codigo = string.Empty;
        if (string.IsNullOrWhiteSpace(bruto))
        {
            return false;
        }

        var limpo = bruto
            .Trim()
            .ToUpperInvariant()
            .Replace("-", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal);

        if (limpo.Length != Tamanho)
        {
            return false;
        }

        foreach (var caractere in limpo)
        {
            if (!Alfabeto.Contains(caractere))
            {
                return false;
            }
        }

        codigo = limpo;
        return true;
    }
}
