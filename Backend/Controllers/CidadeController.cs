using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

[ApiController]
public class CidadeController : ControllerBase
{
    private readonly ICidadeService _cidadeService;
    public CidadeController(ICidadeService cidadeService)
    {
        this._cidadeService = cidadeService;
    }

    [HttpPost("/cidade")]
    public async Task<IActionResult> CreateCidadeAsync(CreateCidadeRequest request)
    {
        var sns = new Cidade("sans", "AB");
        Cidade cidade = await this._cidadeService.CreateCidadeAsync(sns);

        CreateCidadeResponse response = new(1,"a","b");

        return CreatedAtAction(
            "GetCidade",
            new { id = cidade.Id },
            response);
    }

    [HttpGet("/cidade/{id}")]
    public async Task<IActionResult> GetCidadeAsync(int id)
    {
        var cidades = await this._cidadeService.GetCidadeAsync(id);

        if(cidades is null){ return NotFound(); }

        //var response = new GetCidadeResponse(cidade.Id, cidade.Nome, cidade.EstadoSigla);

        return Ok(cidades);
    }
}