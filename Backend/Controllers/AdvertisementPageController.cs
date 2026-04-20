using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("adpage")]
public class AdvertisementPageController : ControllerBase
{
    private readonly IAdPageService _adPageService;
    private readonly AdPageValidator _adPageValidator;
    public AdvertisementPageController(IAdPageService adPageService, AdPageValidator adPageValidator)
    {
        this._adPageService = adPageService;
        this._adPageValidator = adPageValidator;
    }

    [HttpGet]
    [Route("ads")]
    public async Task<IActionResult> GetActiveAdsAsync([FromQuery] GetActiveAdsRequest_QueryParams queryParams)
    {
        (bool isValid, RequestError? error) = await this._adPageValidator.IsValid(queryParams);

        if(isValid == false){
            return this.StatusCode(error!.StatusCode, error.Message);
        }

        List<ActiveAds> activeAds = await this._adPageService.GetActiveAdsAsync(queryParams);
        GetActiveAdsResponse response = new GetActiveAdsResponse(activeAds);
        
        return Ok(response);
    }
}