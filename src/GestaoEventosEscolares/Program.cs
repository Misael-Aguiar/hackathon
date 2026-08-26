using GestaoEventosEscolares.Extensions;
using GestaoEventosEscolares.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10_000_000;
});
builder.Services.AdicionarCamadaDeDados(builder.Configuration);
builder.Services.AdicionarAutenticacaoEAutorizacao();
builder.Services.AdicionarServicosDeAplicacao();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

// Autenticação precisa vir antes da autorização e do middleware de auditoria.
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditoriaAcesso();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.InicializarBancoAsync();

app.Run();
