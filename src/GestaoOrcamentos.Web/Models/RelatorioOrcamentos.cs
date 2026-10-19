namespace GestaoOrcamentos.Web.Models;

public record RelatorioOrcamentoPorSituacao(
    int Situacao,
    string Descricao,
    int QuantidadeOrcamentos,
    decimal ValorTotal);

public record RelatorioOrcamentos(
    DateOnly DataInicial,
    DateOnly DataFinal,
    IReadOnlyList<RelatorioOrcamentoPorSituacao> Situacoes);
