using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using GestaoOrcamentos.Web.Models.Api;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace GestaoOrcamentos.Web.Controllers;

[ApiController]
[Route("api/enderecos")]
public class EnderecosApiController(ViaCepClient viaCepClient) : ControllerBase
{
    [HttpGet("{cep}")]
    [ProducesResponseType<EnderecoCepDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EnderecoCepDto>> Consultar(
        [FromRoute, RegularExpression("^\\d{8}$", ErrorMessage = "O CEP deve conter oito dígitos.")] string cep,
        CancellationToken cancellationToken)
    {
        try
        {
            var endereco = await viaCepClient.ConsultarAsync(cep, cancellationToken);
            return endereco is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "CEP não encontrado.")
                : Ok(endereco);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Indisponivel();
        }
        catch (HttpRequestException)
        {
            return Indisponivel();
        }
        catch (JsonException)
        {
            return Indisponivel();
        }
        catch (InvalidDataException)
        {
            return Indisponivel();
        }
    }

    private ObjectResult Indisponivel() => Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "O serviço de consulta de CEP está indisponível.");
}