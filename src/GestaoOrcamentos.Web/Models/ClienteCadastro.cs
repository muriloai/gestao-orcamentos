using System.ComponentModel.DataAnnotations;

namespace GestaoOrcamentos.Web.Models;

public class ClienteCadastro
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Pessoa de contato")]
    public string? PessoaContato { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefone/WhatsApp")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254)]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(9)]
    [Display(Name = "CEP")]
    public string? Cep { get; set; }

    [StringLength(200)]
    [Display(Name = "Logradouro")]
    public string? Logradouro { get; set; }

    [StringLength(20)]
    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [StringLength(100)]
    [Display(Name = "Complemento")]
    public string? Complemento { get; set; }

    [StringLength(100)]
    [Display(Name = "Bairro")]
    public string? Bairro { get; set; }

    [StringLength(100)]
    [Display(Name = "Cidade")]
    public string? Cidade { get; set; }

    [StringLength(2)]
    [Display(Name = "UF")]
    public string? Uf { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }
}