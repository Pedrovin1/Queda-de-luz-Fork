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

    [HttpGet]
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

}