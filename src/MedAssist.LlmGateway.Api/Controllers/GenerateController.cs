using MedAssist.LlmGateway.Api.Contracts;
using MedAssist.LlmGateway.Api.Providers;
using MedAssist.LlmGateway.Api.Routing;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.LlmGateway.Api.Controllers;

[ApiController]
[Route("v1/generate")]
public class GenerateController : ControllerBase
{
    private readonly ILLMRouter _router;

    public GenerateController(ILLMRouter router)
    {
        _router = router;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Generate response from DeepSeek")]
    [ProducesResponseType(typeof(GenerateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _router.GenerateAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ToProblem(StatusCodes.Status400BadRequest, "Invalid request", ex.Message));
        }
        catch (LlmProviderException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                ToProblem(StatusCodes.Status502BadGateway, "LLM provider error", ex.Message));
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                ToProblem(StatusCodes.Status502BadGateway, "Upstream network error", ex.Message));
        }
    }

    private ProblemDetails ToProblem(int status, string title, string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = HttpContext.Request.Path
        };
}
