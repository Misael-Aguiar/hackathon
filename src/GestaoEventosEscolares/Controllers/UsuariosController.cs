using GestaoEventosEscolares.Authorization;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoEventosEscolares.Controllers;

/// <summary>
/// Cadastro de alunos e professores. Somente o perfil Administrador acessa estas abas.
/// </summary>
[Authorize(Policy = PoliticasAutorizacao.SomenteAdministrador)]
public class UsuariosController : Controller
{
    private readonly IGestaoUsuarioService _gestaoUsuarioService;

    public UsuariosController(IGestaoUsuarioService gestaoUsuarioService)
    {
        _gestaoUsuarioService = gestaoUsuarioService;
    }

    public IActionResult Index() => RedirectToAction(nameof(Alunos));

    [HttpGet]
    public async Task<IActionResult> Alunos(CancellationToken cancellationToken)
    {
        var modelo = await _gestaoUsuarioService.ObterAlunosAsync(cancellationToken);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alunos(GestaoAlunosViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var tela = await _gestaoUsuarioService.ObterAlunosAsync(cancellationToken);
            tela.Cadastro = modelo.Cadastro;
            return View(tela);
        }

        try
        {
            var resultado = await _gestaoUsuarioService.CadastrarAlunoAsync(modelo.Cadastro, cancellationToken);
            TempData["MensagemSucesso"] =
                $"Aluno {resultado.Nome} cadastrado. RM {resultado.RM} · senha inicial {resultado.SenhaInicial}";
            return RedirectToAction(nameof(Alunos));
        }
        catch (InvalidOperationException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            var tela = await _gestaoUsuarioService.ObterAlunosAsync(cancellationToken);
            tela.Cadastro = modelo.Cadastro;
            return View(tela);
        }
    }

    /// <summary>
    /// Soft delete: bloqueia o login sem apagar inscrições/presenças já confirmadas.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirAluno(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _gestaoUsuarioService.ExcluirAlunoAsync(id, User, cancellationToken);
            TempData["MensagemSucesso"] = "Aluno excluído. Inscrições sem presença foram canceladas; o histórico de check-in foi preservado.";
        }
        catch (InvalidOperationException excecao)
        {
            TempData["MensagemErro"] = excecao.Message;
        }

        return RedirectToAction(nameof(Alunos));
    }

    [HttpGet]
    public async Task<IActionResult> Professores(CancellationToken cancellationToken)
    {
        var modelo = await _gestaoUsuarioService.ObterProfessoresAsync(cancellationToken);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Professores(GestaoProfessoresViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var tela = await _gestaoUsuarioService.ObterProfessoresAsync(cancellationToken);
            tela.Cadastro = modelo.Cadastro;
            return View(tela);
        }

        try
        {
            var resultado = await _gestaoUsuarioService.CadastrarProfessorAsync(modelo.Cadastro, cancellationToken);
            TempData["MensagemSucesso"] =
                $"Professor {resultado.Nome} cadastrado. RM {resultado.RM} · senha inicial {resultado.SenhaInicial}";
            return RedirectToAction(nameof(Professores));
        }
        catch (InvalidOperationException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            var tela = await _gestaoUsuarioService.ObterProfessoresAsync(cancellationToken);
            tela.Cadastro = modelo.Cadastro;
            return View(tela);
        }
    }

    /// <summary>
    /// Soft delete do professor: remove vínculos de permissão e preserva eventos/presenças validadas.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirProfessor(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _gestaoUsuarioService.ExcluirProfessorAsync(id, User, cancellationToken);
            TempData["MensagemSucesso"] = "Professor excluído. Vínculos com eventos foram removidos; presenças que ele validou permanecem.";
        }
        catch (InvalidOperationException excecao)
        {
            TempData["MensagemErro"] = excecao.Message;
        }

        return RedirectToAction(nameof(Professores));
    }
}
