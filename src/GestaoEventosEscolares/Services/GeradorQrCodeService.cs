using QRCoder;
using GestaoEventosEscolares.Services.Interfaces;

namespace GestaoEventosEscolares.Services;

public class GeradorQrCodeService : IGeradorQrCodeService
{
    public byte[] GerarPng(string conteudo)
    {
        using var gerador = new QRCodeGenerator();
        using var dados = gerador.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(dados);
        return png.GetGraphic(12, [208, 18, 45], [255, 255, 255]);
    }
}
