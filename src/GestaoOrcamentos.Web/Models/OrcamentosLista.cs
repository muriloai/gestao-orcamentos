namespace GestaoOrcamentos.Web.Models;

public record OrcamentosLista(string? Busca, SituacaoOrcamento? Situacao, IReadOnlyList<Orcamento> Orcamentos);