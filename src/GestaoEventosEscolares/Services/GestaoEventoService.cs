using GestaoEventosEscolares.Models.Entidades;
using GestaoEventosEscolares.Models.Enums;
using GestaoEventosEscolares.Models.ViewModels;
using GestaoEventosEscolares.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GestaoEventosEscolares.Data;
using GestaoEventosEscolares.Extensions;

namespace GestaoEventosEscolares.Services;

public class GestaoEventoService : IGestaoEventoService
{
    private readonly ApplicationDbContext _contexto;
    private readonly UserManager<Usuario> _usuarios;
    private readonly IArmazenamentoImagemEventoService _imagens;

    public GestaoEventoService(
        ApplicationDbContext contexto,
        UserManager<Usuario> usuarios,
        IArmazenamentoImagemEventoService imagens)
    {
        _contexto = contexto;
        _usuarios = usuarios;
        _imagens = imagens;
    }

    public async Task<FormularioEventoViewModel> MontarFormularioNovoAsync(
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var modelo = new FormularioEventoViewModel
        {
            PodeAlterarPermissoes = usuario.EhAdministrador()
        };

        await PreencherOpcoesDeFormularioAsync(modelo, usuario, cancellationToken);
        return modelo;
    }

    public async Task<FormularioEventoViewModel?> MontarFormularioEdicaoAsync(
        int eventoId,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var evento = await _contexto.Eventos
            .Include(item => item.ProfessoresAutorizados)
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken);

        if (evento is null)
        {
            return null;
        }

        var modelo = new FormularioEventoViewModel
        {
            Id = evento.Id,
            Titulo = evento.Titulo,
            Subtitulo = evento.Subtitulo,
            Descricao = evento.Descricao,
            Objetivo = evento.Objetivo,
            InformacoesAdicionais = evento.InformacoesAdicionais,
            Local = evento.Local,
            Data = DateOnly.FromDateTime(evento.DataInicio),
            HoraInicio = TimeOnly.FromDateTime(evento.DataInicio),
            HoraFim = TimeOnly.FromDateTime(evento.DataFim),
            Status = evento.Status,
            CaminhoImagemAtual = evento.CaminhoImagem,
            ProfessoresResponsaveisIds = evento.ProfessoresAutorizados
                .Where(vinculo => vinculo.PodeEditarEvento)
                .Select(vinculo => vinculo.ProfessorId)
                .ToList(),
            PodeAlterarPermissoes = usuario.EhAdministrador()
        };

        await PreencherOpcoesDeFormularioAsync(modelo, usuario, cancellationToken);
        return modelo;
    }

    public async Task PreencherOpcoesDeFormularioAsync(
        FormularioEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        modelo.PodeAlterarPermissoes = usuario.EhAdministrador();
        modelo.ProfessoresDisponiveis = await ListarProfessoresSelectAsync(cancellationToken);
    }

    public async Task<int> CriarAsync(
        FormularioEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        ValidarAgenda(modelo);

        var usuarioId = usuario.ObterId()
            ?? throw new InvalidOperationException("Usuário autenticado sem identificador.");

        var evento = new Evento
        {
            Titulo = modelo.Titulo.Trim(),
            Subtitulo = modelo.Subtitulo.Trim(),
            Descricao = modelo.Descricao.Trim(),
            Objetivo = modelo.Objetivo.Trim(),
            InformacoesAdicionais = modelo.InformacoesAdicionais?.Trim() ?? string.Empty,
            Local = modelo.Local.Trim(),
            DataInicio = CombinarAgenda(modelo.Data, modelo.HoraInicio),
            DataFim = CombinarAgenda(modelo.Data, modelo.HoraFim),
            Status = modelo.Status,
            CriadoPorUsuarioId = usuarioId,
            DataCriacao = DateTime.UtcNow,
            CargaHorariaHoras = Math.Max(1, (int)(modelo.HoraFim - modelo.HoraInicio).TotalHours)
        };

        if (modelo.Imagem is { Length: > 0 })
        {
            evento.CaminhoImagem = await _imagens.SalvarAsync(modelo.Imagem, cancellationToken);
        }

        _contexto.Eventos.Add(evento);
        await _contexto.SaveChangesAsync(cancellationToken);

        var responsaveis = new HashSet<string>(modelo.ProfessoresResponsaveisIds);
        if (usuario.EhProfessor())
        {
            responsaveis.Add(usuarioId);
        }

        await SincronizarResponsaveisAsync(evento.Id, responsaveis, usuarioId, professorProtegidoId: null, cancellationToken);
        await _contexto.SaveChangesAsync(cancellationToken);
        return evento.Id;
    }

    public async Task AtualizarAsync(
        FormularioEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        ValidarAgenda(modelo);

        var evento = await _contexto.Eventos
            .Include(item => item.ProfessoresAutorizados)
            .FirstOrDefaultAsync(item => item.Id == modelo.Id, cancellationToken)
            ?? throw new InvalidOperationException("Evento não encontrado.");

        evento.Titulo = modelo.Titulo.Trim();
        evento.Subtitulo = modelo.Subtitulo.Trim();
        evento.Descricao = modelo.Descricao.Trim();
        evento.Objetivo = modelo.Objetivo.Trim();
        evento.InformacoesAdicionais = modelo.InformacoesAdicionais?.Trim() ?? string.Empty;
        evento.Local = modelo.Local.Trim();
        evento.DataInicio = CombinarAgenda(modelo.Data, modelo.HoraInicio);
        evento.DataFim = CombinarAgenda(modelo.Data, modelo.HoraFim);
        evento.Status = modelo.Status;
        evento.CargaHorariaHoras = Math.Max(1, (int)(modelo.HoraFim - modelo.HoraInicio).TotalHours);

        if (modelo.Imagem is { Length: > 0 })
        {
            var caminhoAnterior = evento.CaminhoImagem;
            evento.CaminhoImagem = await _imagens.SalvarAsync(modelo.Imagem, cancellationToken);
            _imagens.Excluir(caminhoAnterior);
        }

        var usuarioId = usuario.ObterId() ?? string.Empty;
        if (usuario.EhAdministrador())
        {
            await SincronizarResponsaveisAsync(
                evento.Id,
                modelo.ProfessoresResponsaveisIds,
                usuarioId,
                professorProtegidoId: null,
                cancellationToken);
        }

        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<PermissoesEventoViewModel?> ObterPermissoesAsync(
        int eventoId,
        CancellationToken cancellationToken = default)
    {
        var evento = await _contexto.Eventos
            .AsNoTracking()
            .Include(item => item.ProfessoresAutorizados)
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken);

        if (evento is null)
        {
            return null;
        }

        var professores = await ListarProfessoresAsync(cancellationToken);
        var vinculos = evento.ProfessoresAutorizados.ToDictionary(item => item.ProfessorId);

        return new PermissoesEventoViewModel
        {
            EventoId = evento.Id,
            TituloEvento = evento.Titulo,
            Professores = professores.Select(professor =>
            {
                vinculos.TryGetValue(professor.Id, out var vinculo);
                return new PermissaoProfessorItemViewModel
                {
                    ProfessorId = professor.Id,
                    NomeCompleto = professor.NomeCompleto,
                    RM = professor.RM,
                    PodeAcessarPresenca = vinculo?.PodeAcessarPresenca ?? false,
                    PodeEditarEvento = vinculo?.PodeEditarEvento ?? false
                };
            }).ToList()
        };
    }

    public async Task SalvarPermissoesAsync(
        PermissoesEventoViewModel modelo,
        ClaimsPrincipal usuario,
        CancellationToken cancellationToken = default)
    {
        var eventoExiste = await _contexto.Eventos.AnyAsync(item => item.Id == modelo.EventoId, cancellationToken);
        if (!eventoExiste)
        {
            throw new InvalidOperationException("Evento não encontrado.");
        }

        var usuarioId = usuario.ObterId()
            ?? throw new InvalidOperationException("Usuário autenticado sem identificador.");

        var idsValidos = (await ListarProfessoresAsync(cancellationToken))
            .Select(professor => professor.Id)
            .ToHashSet();

        var existentes = await _contexto.ProfessoresAutorizadosEvento
            .Where(vinculo => vinculo.EventoId == modelo.EventoId)
            .ToListAsync(cancellationToken);

        foreach (var item in modelo.Professores.Where(professor => idsValidos.Contains(professor.ProfessorId)))
        {
            var vinculo = existentes.FirstOrDefault(existente => existente.ProfessorId == item.ProfessorId);
            var manter = item.PodeAcessarPresenca || item.PodeEditarEvento;

            if (!manter)
            {
                if (vinculo is not null)
                {
                    _contexto.ProfessoresAutorizadosEvento.Remove(vinculo);
                }

                continue;
            }

            if (vinculo is null)
            {
                _contexto.ProfessoresAutorizadosEvento.Add(new ProfessorAutorizadoEvento
                {
                    EventoId = modelo.EventoId,
                    ProfessorId = item.ProfessorId,
                    AutorizadoPorUsuarioId = usuarioId,
                    DataAutorizacao = DateTime.UtcNow,
                    PodeAcessarPresenca = item.PodeAcessarPresenca,
                    PodeEditarEvento = item.PodeEditarEvento
                });
            }
            else
            {
                vinculo.PodeAcessarPresenca = item.PodeAcessarPresenca;
                vinculo.PodeEditarEvento = item.PodeEditarEvento;
            }
        }

        await _contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task SincronizarResponsaveisAsync(
        int eventoId,
        IEnumerable<string> idsSelecionados,
        string autorizadoPorId,
        string? professorProtegidoId,
        CancellationToken cancellationToken)
    {
        var selecionados = idsSelecionados
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet();

        if (!string.IsNullOrWhiteSpace(professorProtegidoId))
        {
            selecionados.Add(professorProtegidoId);
        }

        var idsProfessores = (await ListarProfessoresAsync(cancellationToken))
            .Select(professor => professor.Id)
            .ToHashSet();

        selecionados.IntersectWith(idsProfessores);

        var existentes = await _contexto.ProfessoresAutorizadosEvento
            .Where(vinculo => vinculo.EventoId == eventoId)
            .ToListAsync(cancellationToken);

        foreach (var vinculo in existentes)
        {
            if (selecionados.Contains(vinculo.ProfessorId))
            {
                vinculo.PodeEditarEvento = true;
                continue;
            }

            vinculo.PodeEditarEvento = false;
            if (!vinculo.PodeAcessarPresenca)
            {
                _contexto.ProfessoresAutorizadosEvento.Remove(vinculo);
            }
        }

        var idsExistentes = existentes.Select(vinculo => vinculo.ProfessorId).ToHashSet();
        foreach (var professorId in selecionados.Except(idsExistentes))
        {
            _contexto.ProfessoresAutorizadosEvento.Add(new ProfessorAutorizadoEvento
            {
                EventoId = eventoId,
                ProfessorId = professorId,
                AutorizadoPorUsuarioId = autorizadoPorId,
                DataAutorizacao = DateTime.UtcNow,
                PodeEditarEvento = true,
                PodeAcessarPresenca = true
            });
        }
    }

    public async Task<ConfirmacaoExclusaoEventoViewModel?> ObterConfirmacaoExclusaoAsync(
        int eventoId,
        CancellationToken cancellationToken = default)
    {
        var evento = await _contexto.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken);

        if (evento is null)
        {
            return null;
        }

        return new ConfirmacaoExclusaoEventoViewModel
        {
            EventoId = evento.Id,
            Titulo = evento.Titulo,
            TotalInscricoes = await _contexto.Inscricoes.CountAsync(item => item.EventoId == eventoId, cancellationToken),
            TotalPresencas = await _contexto.Presencas.CountAsync(item => item.EventoId == eventoId, cancellationToken),
            TotalCertificados = await _contexto.Certificados.CountAsync(item => item.EventoId == eventoId, cancellationToken)
        };
    }

    /// <summary>
    /// Exclusão física: FKs são Restrict, então os filhos são apagados na ordem certa dentro de uma transação.
    /// </summary>
    public async Task ExcluirAsync(int eventoId, CancellationToken cancellationToken = default)
    {
        var evento = await _contexto.Eventos
            .FirstOrDefaultAsync(item => item.Id == eventoId, cancellationToken)
            ?? throw new InvalidOperationException("Evento não encontrado.");

        var caminhoImagem = evento.CaminhoImagem;

        await using var transacao = await _contexto.Database.BeginTransactionAsync(cancellationToken);

        var certificados = await _contexto.Certificados
            .Where(item => item.EventoId == eventoId)
            .ToListAsync(cancellationToken);
        _contexto.Certificados.RemoveRange(certificados);

        var presencas = await _contexto.Presencas
            .Where(item => item.EventoId == eventoId)
            .ToListAsync(cancellationToken);
        _contexto.Presencas.RemoveRange(presencas);

        var inscricoes = await _contexto.Inscricoes
            .Where(item => item.EventoId == eventoId)
            .ToListAsync(cancellationToken);
        _contexto.Inscricoes.RemoveRange(inscricoes);

        var vinculos = await _contexto.ProfessoresAutorizadosEvento
            .Where(item => item.EventoId == eventoId)
            .ToListAsync(cancellationToken);
        _contexto.ProfessoresAutorizadosEvento.RemoveRange(vinculos);

        _contexto.Eventos.Remove(evento);
        await _contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);

        _imagens.Excluir(caminhoImagem);
    }

    private async Task<IReadOnlyList<SelectListItem>> ListarProfessoresSelectAsync(CancellationToken cancellationToken)
    {
        var professores = await ListarProfessoresAsync(cancellationToken);
        return professores
            .Select(professor => new SelectListItem(
                $"{professor.NomeCompleto} (RM {professor.RM})",
                professor.Id))
            .ToList();
    }

    private async Task<IReadOnlyList<Usuario>> ListarProfessoresAsync(CancellationToken cancellationToken)
    {
        var professores = await _usuarios.GetUsersInRoleAsync(NomesPerfis.Professor);
        cancellationToken.ThrowIfCancellationRequested();
        return professores
            .Where(professor => professor.Ativo)
            .OrderBy(professor => professor.NomeCompleto)
            .ToList();
    }

    private static void ValidarAgenda(FormularioEventoViewModel modelo)
    {
        if (modelo.HoraFim <= modelo.HoraInicio)
        {
            throw new InvalidOperationException("O horário de término deve ser posterior ao de início.");
        }
    }

    private static DateTime CombinarAgenda(DateOnly data, TimeOnly hora)
        => DateTime.SpecifyKind(data.ToDateTime(hora), DateTimeKind.Unspecified);
}
