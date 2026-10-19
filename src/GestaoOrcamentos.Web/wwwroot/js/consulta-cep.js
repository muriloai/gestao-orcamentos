document.addEventListener("click", async (event) => {
    const botao = event.target.closest("[data-cep-consultar]");
    if (!botao) {
        return;
    }

    const formulario = botao.closest("form");
    const cep = formulario?.querySelector('[name="Cep"], [name$=".Cep"]');
    const mensagem = botao.parentElement.querySelector("[data-cep-mensagem]");
    const numeros = cep?.value.replace(/\D/g, "") ?? "";
    if (numeros.length !== 8) {
        mensagem.textContent = "Informe um CEP com oito dígitos. Você também pode preencher o endereço manualmente.";
        return;
    }

    const textoOriginal = botao.textContent;
    botao.disabled = true;
    botao.textContent = "Consultando…";
    mensagem.textContent = "";

    try {
        const resposta = await fetch(`/api/enderecos/${numeros}`, { headers: { Accept: "application/json" } });
        if (resposta.status === 404) {
            mensagem.textContent = "CEP não encontrado. Você pode preencher o endereço manualmente.";
            return;
        }
        if (!resposta.ok) {
            mensagem.textContent = "Consulta indisponível. Você pode preencher o endereço manualmente.";
            return;
        }

        const endereco = await resposta.json();
        const prefixo = cep.name.endsWith("Cep") ? cep.name.slice(0, -3) : "";
        const preencher = (nome, valor) => {
            const campo = formulario.querySelector(`[name="${prefixo}${nome}"]`);
            if (campo) {
                campo.value = valor ?? "";
            }
        };

        cep.value = endereco.cep;
        preencher("Logradouro", endereco.logradouro);
        preencher("Bairro", endereco.bairro);
        preencher("Cidade", endereco.cidade);
        preencher("Uf", endereco.uf);
        mensagem.textContent = "Endereço preenchido. Confira os dados e complete número e complemento.";
    } catch {
        mensagem.textContent = "Consulta indisponível. Você pode preencher o endereço manualmente.";
    } finally {
        botao.disabled = false;
        botao.textContent = textoOriginal;
    }
});
