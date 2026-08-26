using QRCoder;
using GestaoEventosEscolares.Services.Interfaces;

namespace GestaoEventosEscolares.Services;

public class GeradorQrCodeService : IGeradorQrCodeService
{
    /// <summary>
    /// Módulos pretos em fundo branco (melhor leitura). A cor vermelha fica na moldura da view.
    /// ECC H recupera até ~30% de dano no papel.
    /// </summary>
    public byte[] GerarPng(string conteudo)
    {
        using var gerador = new QRCodeGenerator();
        using var dados = gerador.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.H);
        var png = new PngByteQRCode(dados);
        return png.GetGraphic(
            pixelsPerModule: 20,
            darkColorRgba: [10, 10, 10],
            lightColorRgba: [255, 255, 255],
            drawQuietZones: true);
    }
}
