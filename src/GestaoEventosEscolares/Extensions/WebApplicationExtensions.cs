using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InicializarBancoAsync(this WebApplication app)
    {
        using var escopo = app.Services.CreateScope();
        var servicos = escopo.ServiceProvider;
        var logger = servicos.GetRequiredService<ILoggerFactory>().CreateLogger("InicializacaoBanco");

        try
        {
            var contexto = servicos.GetRequiredService<ApplicationDbContext>();
            await contexto.Database.MigrateAsync();
            await DatabaseSeeder.PopularAsync(servicos);
        }
        catch (Exception excecao)
        {
            logger.LogError(excecao, "Falha ao aplicar migrations ou popular o banco de dados.");
            throw;
        }
    }
}
