namespace GestaoOrcamentos.Web.Models;

public record AvisosOrcamentos(DateOnly DataReferencia, IReadOnlyList<ResumoOrcamento> Orcamentos);
