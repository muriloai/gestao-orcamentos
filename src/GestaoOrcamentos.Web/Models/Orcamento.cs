namespace GestaoOrcamentos.Web.Models;

public class Orcamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public required string Titulo { get; set; }
    public DateOnly DataEmissao { get; set; }
    public DateOnly? Validade { get; set; }
    public string? Observacoes { get; set; }
    public List<ItemOrcamento> Itens { get; set; } = [];

    public string Numero => Id.ToString("D6");
    public decimal Total => Itens.Sum(item => item.Subtotal);
}