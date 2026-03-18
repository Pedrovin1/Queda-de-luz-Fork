using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

[ApiController]
[Route("homepage")]
public class HomePageController : ControllerBase
{
    private readonly IHomePageService _homePageService;
    public HomePageController(IHomePageService homePageService)
    {
        this._homePageService = homePageService;
    }

    [HttpGet]
    [Route("city/{city_id}/districts")]
    public async Task<IActionResult> GetDistrictsAsync(int city_id)
    {
        List<District> districts = await this._homePageService.GetDistrictsAsync(city_id);

        GetDistrictsResponse response = DistrictMapping.ToGetDistrictsResponse(districts);

        return Ok(response);
    }

    [HttpGet("/cidade/{id}")]
    public async Task<IActionResult> GetCidadeAsync(int id)
    {
        var cidades = await this._homePageService.GetCidadeAsync(id);

        if(cidades is null){ return NotFound(); }

        //var response = new GetCidadeResponse(cidade.Id, cidade.Nome, cidade.EstadoSigla);

        return Ok(cidades);
    }
}