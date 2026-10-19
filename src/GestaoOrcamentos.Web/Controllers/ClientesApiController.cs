using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Models.Api;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace GestaoOrcamentos.Web.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesApiController(ClienteService clienteService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ClientesPaginaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientesPaginaDto>> Listar(
        [FromQuery] string? nome,
        [FromQuery, Range(1, int.MaxValue)] int pagina = 1,
        [FromQuery, Range(1, 100)] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var (clientes, totalItens) = await clienteService.ListarPaginadoAsync(
            nome, pagina, tamanhoPagina, cancellationToken);
        var totalPaginas = totalItens / tamanhoPagina + (totalItens % tamanhoPagina == 0 ? 0 : 1);
        return Ok(new ClientesPaginaDto(
            clientes.Select(ClienteDto.DeCliente).ToList(), pagina, tamanhoPagina, totalItens, totalPaginas));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Obter(int id, CancellationToken cancellationToken)
    {
        var cliente = await clienteService.ObterAsync(id, cancellationToken);
        return cliente is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Cliente não encontrado.")
            : Ok(ClienteDto.DeCliente(cliente));
    }

    [HttpPost]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteDto>> Criar(
        [FromBody] ClienteFormulario formulario, CancellationToken cancellationToken)
    {
        var id = await clienteService.CadastrarAsync(formulario, cancellationToken);
        var cliente = (await clienteService.ObterAsync(id, cancellationToken))!;
        return CreatedAtAction(nameof(Obter), new { id }, ClienteDto.DeCliente(cliente));
    }
}