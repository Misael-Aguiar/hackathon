using GestaoEventosEscolares.Models.Enums;

namespace GestaoEventosEscolares.Models.ViewModels;

public class PerfilAlunoViewModel
{
    public string NomeCompleto { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int TotalInscricoes { get; set; }

    public int TotalPresencas { get; set; }

    public int TotalCertificados { get; set; }

    public IReadOnlyList<ParticipacaoEventoViewModel> Historico { get; set; } = [];
}

public class ParticipacaoEventoViewModel
{
    public int InscricaoId { get; set; }

    public int EventoId { get; set; }

    public string TituloEvento { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }

    public int CargaHorariaHoras { get; set; }

    public StatusParticipacao Status { get; set; }

    public bool PodeBaixarCertificado { get; set; }
}

public class DadosCertificadoPdf
{
    public string NomeAluno { get; set; } = string.Empty;

    public string RM { get; set; } = string.Empty;

    public string TituloEvento { get; set; } = string.Empty;

    public int CargaHorariaHoras { get; set; }

    public DateTime DataEvento { get; set; }

    public DateTime DataEmissao { get; set; }

    public string CodigoVerificacao { get; set; } = string.Empty;
}

public class ArquivoCertificado
{
    public byte[] Conteudo { get; init; } = [];

    public string NomeArquivo { get; init; } = "certificado.pdf";
}
