namespace GestaoOrcamentos.Web.Models;

public class ResumoOrcamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public required string NomeCliente { get; set; }
    public required string Titulo { get; set; }
    public DateOnly DataEmissao { get; set; }
    public DateOnly? Validade { get; set; }
    public SituacaoOrcamento Situacao { get; set; }
    public decimal Total { get; set; }

    public string Numero => Id.ToString("D6");
    public string SituacaoTexto => Situacao switch
    {
        SituacaoOrcamento.Rascunho => "Rascunho",
        SituacaoOrcamento.Pendente => "Pendente de aprovação",
        SituacaoOrcamento.Aprovado => "Aprovado",
        SituacaoOrcamento.Recusado => "Recusado",
        _ => throw new InvalidOperationException("Situação de orçamento desconhecida.")
    };
}
