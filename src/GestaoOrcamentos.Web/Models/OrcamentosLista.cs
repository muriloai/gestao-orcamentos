namespace GestaoOrcamentos.Web.Models;

public record OrcamentosLista(string? Busca, IReadOnlyList<Orcamento> Orcamentos);