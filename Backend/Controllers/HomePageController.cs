using Microsoft.AspNetCore.Mvc;

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
    [Route("cities/{city_id}/districts")]
    public async Task<IActionResult> GetDistrictsAsync(int city_id)
    {
        List<District> districts = await this._homePageService.GetDistrictsAsync(city_id);
        if(districts is null || districts.Count <= 0){ return NotFound(); }

        GetDistrictsResponse response = DistrictMapping.ToGetDistrictsResponse(districts);

        return Ok(response);
    }


    [HttpPost]
    [Route("cities/{city_id}/districts/{district_id}/reports")]
    public async Task<IActionResult> PostReportAsync(int city_id, int district_id, PostReportRequest request)
    {
        //<<TODO: to validate city--district relation>>
        //<<TODO: if userId not null, then validate its existence>>

        Report report = request.ToReport(district_id);
        report = await this._homePageService.PostReportAsync(report);

        PostReportResponse response = report.ToPostReportResponse();
        
        //<<TODO: to create a GET endpoint to access reports>>
        return Ok(response);
    }

    [HttpGet]
    [Route("cities/{city_id}/statistics")]
    public async Task<IActionResult> GetCityStatistics(int city_id)
    {
        //<<TODO: to validate city existence>>
        GetCityStatisticsResponse response = await this._homePageService.GetCityStatistics(city_id);

        return Ok(response);
    }


    [HttpGet]
    [Route("states/{state_abbreviation}/cities")]
    public async Task<IActionResult> GetCitiesAsync(string state_abbreviation)
    {
        state_abbreviation = state_abbreviation.ToUpper();
        GetCitiesResponse? response = await this._homePageService.GetCitiesAsync(state_abbreviation);

        if(response is null){ return NotFound(); }

        return Ok(response);
    }
}