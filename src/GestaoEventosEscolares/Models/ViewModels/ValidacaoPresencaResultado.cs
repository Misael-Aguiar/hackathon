namespace GestaoEventosEscolares.Models.ViewModels;

public class LeituraQrViewModel
{
    public string Codigo { get; set; } = string.Empty;
}

public class ValidacaoPresencaResultado
{
    public bool Sucesso { get; set; }

    public string Mensagem { get; set; } = string.Empty;

    public string? NomeAluno { get; set; }

    public string? RM { get; set; }

    public DateTime? Horario { get; set; }

    public static ValidacaoPresencaResultado Falha(string mensagem)
        => new() { Sucesso = false, Mensagem = mensagem };

    public static ValidacaoPresencaResultado Ok(string nome, string rm, DateTime horario)
        => new()
        {
            Sucesso = true,
            Mensagem = "Presença registrada.",
            NomeAluno = nome,
            RM = rm,
            Horario = horario
        };
}
