namespace GestaoEventosEscolares.Services.Interfaces;

public interface IArmazenamentoImagemEventoService
{
    Task<string> SalvarAsync(IFormFile arquivo, CancellationToken cancellationToken = default);

    void Excluir(string? caminhoRelativo);
}
