using System.ComponentModel.DataAnnotations;

namespace GestaoOrcamentos.Web.Models;

public enum SituacaoOrcamento
{
    [Display(Name = "Rascunho")]
    Rascunho,

    [Display(Name = "Pendente de aprovação")]
    Pendente,

    [Display(Name = "Aprovado")]
    Aprovado,

    [Display(Name = "Recusado")]
    Recusado
}