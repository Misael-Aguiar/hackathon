document.addEventListener("DOMContentLoaded", () => {
    configurarPreviewImagem();
    configurarBuscaPresenca();
});

function configurarPreviewImagem() {
    const input = document.getElementById("Imagem");
    const preview = document.getElementById("previewImagem");
    if (!input || !preview) {
        return;
    }

    input.addEventListener("change", () => {
        const arquivo = input.files && input.files[0];
        if (arquivo) {
            preview.src = URL.createObjectURL(arquivo);
        }
    });
}

function configurarBuscaPresenca() {
    const campo = document.getElementById("buscaPresenca");
    if (!campo) {
        return;
    }

    const linhas = document.querySelectorAll("[data-busca]");
    const vazio = document.getElementById("buscaPresencaVazio");

    const filtrar = () => {
        const termo = campo.value.trim().toLowerCase();
        let visiveis = 0;
        linhas.forEach((linha) => {
            const texto = (linha.getAttribute("data-busca") || "").toLowerCase();
            const ok = !termo || texto.includes(termo);
            linha.hidden = !ok;
            if (ok) {
                visiveis += 1;
            }
        });
        if (vazio) {
            vazio.hidden = visiveis !== 0 || linhas.length === 0;
        }
    };

    campo.addEventListener("input", filtrar);
}
