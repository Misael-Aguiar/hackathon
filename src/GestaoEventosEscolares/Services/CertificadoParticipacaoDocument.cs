using GestaoEventosEscolares.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestaoEventosEscolares.Services;

/// <summary>
/// Template visual do certificado: paleta preto, vermelho e branco em A4 paisagem.
/// </summary>
public class CertificadoParticipacaoDocument : IDocument
{
    private static readonly string Vermelho = "#D0122D";
    private static readonly string Preto = "#0A0A0A";
    private static readonly string Papel = "#F6F4F2";

    private readonly DadosCertificadoPdf _dados;

    public CertificadoParticipacaoDocument(DadosCertificadoPdf dados)
    {
        _dados = dados;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Certificado — {_dados.TituloEvento}",
        Author = "Gestão de Eventos Escolares"
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(0);
            page.PageColor(Papel);

            page.Content().Row(row =>
            {
                row.ConstantItem(22).Background(Vermelho);

                row.RelativeItem().Padding(36).Column(coluna =>
                {
                    coluna.Item().Row(topo =>
                    {
                        topo.ConstantItem(10).Height(28).Background(Vermelho);
                        topo.RelativeItem().PaddingLeft(12).Column(marca =>
                        {
                            marca.Item().Text("EVENTOS ESCOLARES")
                                .FontColor(Preto)
                                .FontSize(11)
                                .LetterSpacing(0.18f)
                                .SemiBold();
                            marca.Item().Text("CERTIFICADO DE PARTICIPAÇÃO")
                                .FontColor(Vermelho)
                                .FontSize(28)
                                .Bold();
                        });
                    });

                    coluna.Item().PaddingTop(18).LineHorizontal(1).LineColor(Vermelho);

                    coluna.Item().PaddingTop(28).Text("Certificamos que")
                        .FontColor(Preto)
                        .FontSize(13);

                    coluna.Item().PaddingTop(8).Text(_dados.NomeAluno)
                        .FontColor(Preto)
                        .FontSize(28)
                        .Bold();

                    coluna.Item().Text($"RM {_dados.RM}")
                        .FontColor("#666666")
                        .FontSize(11);

                    coluna.Item().PaddingTop(18).Text(texto =>
                    {
                        texto.Span("participou do evento ").FontColor(Preto).FontSize(13);
                        texto.Span(_dados.TituloEvento).FontColor(Vermelho).FontSize(13).Bold();
                        texto.Span($" realizado em {_dados.DataEvento:dd/MM/yyyy}, com carga horária de ").FontColor(Preto).FontSize(13);
                        texto.Span($"{_dados.CargaHorariaHoras} hora(s)").FontColor(Preto).FontSize(13).Bold();
                        texto.Span(".").FontColor(Preto).FontSize(13);
                    });

                    coluna.Item().PaddingTop(36).Row(rodape =>
                    {
                        rodape.RelativeItem().Column(bloco =>
                        {
                            bloco.Item().Text("Emitido em")
                                .FontColor("#666666")
                                .FontSize(9);
                            bloco.Item().Text(_dados.DataEmissao.ToString("dd/MM/yyyy"))
                                .FontColor(Preto)
                                .FontSize(12)
                                .SemiBold();
                        });

                        rodape.RelativeItem().AlignRight().Column(bloco =>
                        {
                            bloco.Item().AlignRight().Text("Código de verificação")
                                .FontColor("#666666")
                                .FontSize(9);
                            bloco.Item().AlignRight().Text(_dados.CodigoVerificacao)
                                .FontColor(Vermelho)
                                .FontSize(12)
                                .SemiBold();
                        });
                    });
                });
            });
        });
    }
}
