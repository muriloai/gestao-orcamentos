namespace GestaoOrcamentos.Web.Models;

public record ClienteDetalhes(Cliente Cliente, IReadOnlyList<Orcamento> Orcamentos);