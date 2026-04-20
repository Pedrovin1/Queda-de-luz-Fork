using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private IAccountService _accountService;
    private AccountValidator _validator;

    public AccountController(IAccountService service, AccountValidator validator)
    {
        this._accountService = service;
        this._validator = validator;
    } 

    [HttpPost]
    public async Task<IActionResult> PostAccountAsync(PostAccountRequest request)
    {
        (bool isValid, var error) = await this._validator.IsValid(request);
        if(isValid == false)
        { 
            return this.StatusCode(error!.StatusCode, error.Message); 
        }

        string hashedPassword = this._accountService.HashPassword(request.Unhashed_Password);
        
        BaseAccount account;
        if(request.Person_Details is not null){
            account = request.ToPersonAccount(hashedPassword);
        }
        else{
            account = request.ToBusinessAccount(hashedPassword);
        }

        BaseAccount createdAccount;
        try{
            createdAccount = await this._accountService.CreateAccountAsync(account);
        }
        catch(Exception){
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
        
        PostAccountResponse response = createdAccount.ToPostAccountResponse();

        return Ok(response);
        //return Created();
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAccountGetTokenAsync(LoginAccountRequest request)
    {
        string token;
        (bool isValid, var error) =  this._validator.IsValid(request);
        if(isValid == false)
        { 
            return this.StatusCode(error!.StatusCode, error.Message); 
        }

        try{
            (token, error) = await this._accountService.LoginAccountGetTokenAsync(request);
        }
        catch(Exception){
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
        
        if(error is not null)
        { 
            return this.StatusCode(error!.StatusCode, error.Message); 
        }

        LoginAccountResponse response = new LoginAccountResponse(token);
        return Ok(response);
    } 

    [HttpGet]
    [Authorize]
    [Route("login")]
    public async Task<IActionResult> GetLoginTokenData()
    {
        string? clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? clientTypeClaim = User.FindFirstValue(ClaimTypes.Role);

        if(clientIdClaim is null || clientTypeClaim is null){
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Invalid Token Format");
        }

        GetLoginTokenDataResponse response = new(
            int.Parse(clientIdClaim),
            clientTypeClaim
        );

        return Ok(response);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{account_id}")]
    public async Task<IActionResult> GetAccountData(int account_id)
    {
        RequestError? error = null;
        GetAccountDataResponse response;

        string? clientIdClaim = null;
        string? clientAccountTypeClaim = null;
        if(User.Identity is not null && User.Identity.IsAuthenticated == true)
        {
            clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? null;
            clientAccountTypeClaim = User.FindFirstValue(ClaimTypes.Role) ?? null;
            if(clientIdClaim is null){
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Invalid token format");
            }
        }

        int? parsedClientId = clientIdClaim is null ? null
                                                      : int.Parse(clientIdClaim);

        bool isRequestingSelfData = (account_id == parsedClientId);
        string? accountType = clientAccountTypeClaim;

        if(isRequestingSelfData == false)
        {
            bool accountExists;
            (accountExists, accountType, error) = await this._validator.AccountExistsAsync(account_id);  
            if(accountExists == false)
            {
                return this.StatusCode(error!.StatusCode, error.Message);
            }
        }
        
        (response, error) = await this._accountService.GetAccountData(account_id, accountType!, isRequestingSelfData);
        
        if(error is not null)
        {
            return this.StatusCode(error!.StatusCode, error.Message);
        }

        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    [Route("{account_id}/ads")]
    public async Task<IActionResult> PostAdvertisementAsync(PostAdvertisementRequest request, int account_id)
    {
        RequestError? error;
        string? clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? null;
        int? parsedClientId = clientIdClaim is null ? null
                                                    : int.Parse(clientIdClaim);

        if(account_id != parsedClientId){
            return this.StatusCode(StatusCodes.Status403Forbidden, 
            "You Cannot Create an Ad for another account");
        }

        string accountType = User.FindFirstValue(ClaimTypes.Role)!;

        (bool isValid, error) = this._validator.IsValid(request);
        if(isValid == false){
            return this.StatusCode(error!.StatusCode, error.Message);
        }

        PostAdvertisementResponse response;
        Advertisement? result;
        try{
            (result, error) = await this._accountService.PostAdvertisementAsync(request, (int)parsedClientId, accountType);
        }
        catch(Exception){
            return this.StatusCode(StatusCodes.Status500InternalServerError, "server error");
        }

        if(error is not null){
            return this.StatusCode(error.StatusCode, error.Message);
        }

        response = result!.ToPostAdvertisementResponse();
        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    [Route("{account_id}/ads/{ad_id}/boost")]
    //Placeholder Endpoint Logic
    public async Task<IActionResult> PostBoostAdvertisementAsync(int account_id, int ad_id)
    {
        RequestError? error;
        string? clientIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? null;
        int? parsedClientId = clientIdClaim is null ? null
                                                    : int.Parse(clientIdClaim);

        if(account_id != parsedClientId){
            return this.StatusCode(StatusCodes.Status403Forbidden, 
            "You Cannot Boost the Ad of another account");
        }

        (bool accountExists, var _, error) = await this._validator.AccountExistsAsync(account_id);
        if(accountExists == false){
            return this.StatusCode(error!.StatusCode, error.Message);
        }
        
        (bool isValid, error) = await this._validator.IsAdValidToBoostAsync(ad_id);
        if(isValid == false){
            return this.StatusCode(error!.StatusCode, error.Message);
        }

        (PostBoostAdvertisementReponse? response, error) = await this._accountService.BoostAdvertisementAsync(ad_id);

        if(error is not null){
            return this.StatusCode(error.StatusCode, error.Message);
        }

        return Ok(response);
    }
}