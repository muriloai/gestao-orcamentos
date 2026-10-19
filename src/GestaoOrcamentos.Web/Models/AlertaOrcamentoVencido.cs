namespace GestaoOrcamentos.Web.Models;

public class AlertaOrcamentoVencido
{
    public int OrcamentoId { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public Orcamento Orcamento { get; set; } = null!;
}
