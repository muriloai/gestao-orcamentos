using System.ComponentModel.DataAnnotations;

namespace GestaoOrcamentos.Web.Models;

public record class OrcamentoFormulario : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um cliente.")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(150)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de emissão.")]
    [Display(Name = "Data de emissão")]
    public DateOnly? DataEmissao { get; set; }

    [Display(Name = "Validade")]
    public DateOnly? Validade { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    public List<ItemOrcamentoFormulario> Itens { get; set; } = [new()];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Itens is null || Itens.Count == 0)
        {
            yield return new ValidationResult("Adicione pelo menos um item.", [nameof(Itens)]);
        }
    }
}

public class ItemOrcamentoFormulario
{
    [Required(ErrorMessage = "Informe a descrição.")]
    [StringLength(200)]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Range(typeof(decimal), "0,001", "999999,999", ErrorMessage = "A quantidade deve ser maior que zero e ter até três casas decimais.")]
    [Display(Name = "Quantidade")]
    public decimal Quantidade { get; set; } = 1;

    [Required(ErrorMessage = "Informe a unidade.")]
    [StringLength(30)]
    [Display(Name = "Unidade")]
    public string Unidade { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999,99", ErrorMessage = "O preço deve ser zero ou positivo e ter até duas casas decimais.")]
    [Display(Name = "Preço unitário")]
    public decimal PrecoUnitario { get; set; }
}