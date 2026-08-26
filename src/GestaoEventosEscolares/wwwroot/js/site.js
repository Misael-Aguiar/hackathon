document.addEventListener("DOMContentLoaded", () => {
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
});
