using GestaoEventosEscolares.Models.ViewModels;
using System.Security.Claims;

namespace GestaoEventosEscolares.Services.Interfaces;

public interface IGestaoUsuarioService
{
    Task<GestaoAlunosViewModel> ObterAlunosAsync(CancellationToken cancellationToken = default);

    Task<ResultadoCadastroUsuario> CadastrarAlunoAsync(
        CadastroAlunoViewModel modelo,
        CancellationToken cancellationToken = default);

    Task ExcluirAlunoAsync(string id, ClaimsPrincipal administrador, CancellationToken cancellationToken = default);

    Task<EditarAlunoViewModel?> ObterEdicaoAlunoAsync(string id, CancellationToken cancellationToken = default);

    Task EditarAlunoAsync(EditarAlunoViewModel modelo, CancellationToken cancellationToken = default);

    Task<GestaoProfessoresViewModel> ObterProfessoresAsync(CancellationToken cancellationToken = default);

    Task<ResultadoCadastroUsuario> CadastrarProfessorAsync(
        CadastroProfessorViewModel modelo,
        CancellationToken cancellationToken = default);

    Task ExcluirProfessorAsync(string id, ClaimsPrincipal administrador, CancellationToken cancellationToken = default);
}
