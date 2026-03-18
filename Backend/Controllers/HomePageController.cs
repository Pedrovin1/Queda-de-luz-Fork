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
        if(districts is null || districts.Count <= 0){ return NotFound(); }

        GetDistrictsResponse response = DistrictMapping.ToGetDistrictsResponse(districts);

        return Ok(response);
    }


    [HttpPost]
     [Route("city/{city_id}/district/{district_id}/reports")]
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
}