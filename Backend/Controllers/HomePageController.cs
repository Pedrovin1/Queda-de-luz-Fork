using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("homepage")]
public class HomePageController : ControllerBase
{
    private readonly IHomePageService _homePageService;
    private readonly HomePageValidator _homePageValidator;
    public HomePageController(IHomePageService homePageService, HomePageValidator homePageValidator)
    {
        this._homePageService = homePageService;
        this._homePageValidator = homePageValidator;
    }

    [HttpGet]
    [Route("cities/{city_id}/districts")]
    public async Task<IActionResult> GetDistrictsAsync(int city_id)
    {
        (List<District> districts, var error) = await this._homePageService.GetDistrictsAsync(city_id);

        if(error is not null)
        {
            return this.StatusCode(error.StatusCode, error.Message);
        }

        GetDistrictsResponse response = DistrictMapping.ToGetDistrictsResponse(districts);

        return Ok(response);
    }

    // [Authorize("default")]
    [HttpPost]
    [Route("cities/{city_id}/districts/{district_id}/reports")]
    [AllowAnonymous] //skip authorization process, but still validates the JWT token
    public async Task<IActionResult> PostReportAsync(int city_id, int district_id, PostReportRequest request)
    {
        RequestError? error = null;
        (bool isValid, error) = await this._homePageValidator.IsValid(request, city_id, district_id);

        if(isValid == false)
        {
            return this.StatusCode(error!.StatusCode, error.Message);
        }

        string? accountIdClaim = null;
        //verifies if it the HTTP request has a valid Authorization Header
        if(User.Identity is not null && User.Identity.IsAuthenticated == true)
        {
            accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? null;
            if(accountIdClaim is null){
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Invalid token format");
            }
        }

        int? parsedAccountId = accountIdClaim is null ? null
                                                      : int.Parse(accountIdClaim);

        Report report = request.ToReport(district_id, parsedAccountId);
        try
        {
            report = await this._homePageService.PostReportAsync(report);
        }
        catch(Exception){
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
        
        PostReportResponse response = report.ToPostReportResponse();
        return Ok(response);
    }

    [HttpGet]
    [Route("cities/{city_id}/statistics")]
    public async Task<IActionResult> GetCityStatistics(int city_id)
    {
        (GetCityStatisticsResponse? response, var error) = await this._homePageService.GetCityStatisticsAsync(city_id);

        if(error is not null)
        {
            return this.StatusCode(error.StatusCode, error.Message);
        }

        return Ok(response);
    }


    [HttpGet]
    [Route("states/{state_abbreviation}/cities")]
    public async Task<IActionResult> GetCitiesAsync(string state_abbreviation)
    {
        state_abbreviation = state_abbreviation.ToUpper();
        (GetCitiesResponse? response, var error) = await this._homePageService.GetCitiesAsync(state_abbreviation);

        if(error is not null)
        { 
            return this.StatusCode(error.StatusCode, error.Message); 
        }

        return Ok(response);
    }
}