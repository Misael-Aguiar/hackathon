namespace GestaoEventosEscolares.Services.Interfaces;

public interface IGeradorQrCodeService
{
    byte[] GerarPng(string conteudo);
}
