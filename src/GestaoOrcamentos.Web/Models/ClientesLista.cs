namespace GestaoOrcamentos.Web.Models;

public record ClientesLista(string? Busca, IReadOnlyList<Cliente> Clientes);