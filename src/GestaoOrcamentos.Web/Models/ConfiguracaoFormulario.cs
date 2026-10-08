using System.ComponentModel.DataAnnotations;

namespace GestaoOrcamentos.Web.Models;

public class ConfiguracaoFormulario
{
    [Required(ErrorMessage = "Informe o nome do negócio.")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    [Display(Name = "Nome do negócio")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(30)]
    [Display(Name = "Telefone")]
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

    public static ConfiguracaoFormulario DoNegocio(ConfiguracaoNegocio negocio) => new()
    {
        Nome = negocio.Nome,
        Telefone = negocio.Telefone,
        Email = negocio.Email,
        Cep = negocio.Cep,
        Logradouro = negocio.Logradouro,
        Numero = negocio.Numero,
        Complemento = negocio.Complemento,
        Bairro = negocio.Bairro,
        Cidade = negocio.Cidade,
        Uf = negocio.Uf
    };

    public void AplicarEm(ConfiguracaoNegocio negocio)
    {
        negocio.Nome = Nome.Trim();
        negocio.Telefone = Telefone?.Trim();
        negocio.Email = Email?.Trim();
        negocio.Cep = Cep?.Trim();
        negocio.Logradouro = Logradouro?.Trim();
        negocio.Numero = Numero?.Trim();
        negocio.Complemento = Complemento?.Trim();
        negocio.Bairro = Bairro?.Trim();
        negocio.Cidade = Cidade?.Trim();
        negocio.Uf = Uf?.Trim();
    }
}