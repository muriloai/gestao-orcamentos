namespace GestaoOrcamentos.Web.Models;

public class ItemOrcamento
{
    public int Id { get; set; }
    public int OrcamentoId { get; set; }
    public required string Descricao { get; set; }
    public decimal Quantidade { get; set; }
    public required string Unidade { get; set; }
    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => decimal.Round(Quantidade * PrecoUnitario, 2, MidpointRounding.AwayFromZero);
}