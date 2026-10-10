namespace GestaoOrcamentos.Web.Models;

public record OrcamentoImpressao(
    Orcamento Orcamento,
    DadosParteOrcamento Cliente,
    DadosParteOrcamento? Negocio);