namespace GestaoEventosEscolares.Extensions;

public static class ImagemEvento
{
    public const string Placeholder = "/img/evento-placeholder.svg";

    public static string Resolver(string? caminhoRelativo)
        => string.IsNullOrWhiteSpace(caminhoRelativo) ? Placeholder : caminhoRelativo;
}
