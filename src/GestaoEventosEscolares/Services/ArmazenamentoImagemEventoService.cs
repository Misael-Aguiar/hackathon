using GestaoEventosEscolares.Services.Interfaces;

namespace GestaoEventosEscolares.Services;

public class ArmazenamentoImagemEventoService : IArmazenamentoImagemEventoService
{
    private const string PastaPublica = "uploads/eventos";
    private const long TamanhoMaximoBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> ExtensoesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> TiposPermitidos = ["image/jpeg", "image/png", "image/webp"];

    private readonly IWebHostEnvironment _ambiente;

    public ArmazenamentoImagemEventoService(IWebHostEnvironment ambiente)
    {
        _ambiente = ambiente;
    }

    public async Task<string> SalvarAsync(IFormFile arquivo, CancellationToken cancellationToken = default)
    {
        if (arquivo.Length <= 0)
        {
            throw new InvalidOperationException("A imagem enviada está vazia.");
        }

        if (arquivo.Length > TamanhoMaximoBytes)
        {
            throw new InvalidOperationException("A imagem deve ter no máximo 5 MB.");
        }

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        var tipo = arquivo.ContentType.ToLowerInvariant();
        if (!ExtensoesPermitidas.Contains(extensao) || !TiposPermitidos.Contains(tipo))
        {
            throw new InvalidOperationException("Envie uma imagem nos formatos JPG, PNG ou WEBP.");
        }

        var pastaFisica = Path.Combine(_ambiente.WebRootPath, "uploads", "eventos");
        Directory.CreateDirectory(pastaFisica);

        var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
        var caminhoFisico = Path.Combine(pastaFisica, nomeArquivo);

        await using var saida = new FileStream(caminhoFisico, FileMode.CreateNew);
        await arquivo.CopyToAsync(saida, cancellationToken);

        return $"/{PastaPublica}/{nomeArquivo}";
    }

    public void Excluir(string? caminhoRelativo)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativo))
        {
            return;
        }

        var relativo = caminhoRelativo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var prefixo = Path.Combine("uploads", "eventos");
        if (!relativo.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var caminhoFisico = Path.Combine(_ambiente.WebRootPath, relativo);
        if (File.Exists(caminhoFisico))
        {
            File.Delete(caminhoFisico);
        }
    }
}
