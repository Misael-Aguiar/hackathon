using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoEventosEscolares.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly UserManager<Usuario> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<Usuario> signInManager,
        UserManager<Usuario> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Painel");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var rm = modelo.RM.Trim();
        var usuario = await _userManager.Users.FirstOrDefaultAsync(item => item.RM == rm);

        if (usuario is null || !usuario.Ativo)
        {
            ModelState.AddModelError(string.Empty, "RM ou senha inválidos.");
            return View(modelo);
        }

        // UserName do Identity é o próprio RM, então o login usa a matrícula como identificador.
        var resultado = await _signInManager.PasswordSignInAsync(
            usuario.UserName!,
            modelo.Senha,
            modelo.LembrarMe,
            lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            _logger.LogInformation("Login realizado. RM={RM} Perfil={Perfil}", usuario.RM, usuario.Perfil);
            return RedirecionarLocalmente(returnUrl);
        }

        if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Conta temporariamente bloqueada. Tente novamente em alguns minutos.");
            return View(modelo);
        }

        ModelState.AddModelError(string.Empty, "RM ou senha inválidos.");
        return View(modelo);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AcessoNegado()
    {
        return View();
    }

    private IActionResult RedirecionarLocalmente(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Painel");
    }
}
