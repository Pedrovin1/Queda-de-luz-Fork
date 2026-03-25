using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("accounts")]
public class AccountSignInOutController : ControllerBase
{
    private IAccountSignInOutService _accountService;
    private AccountSignInOutValidator _validator;

    public AccountSignInOutController(IAccountSignInOutService service, AccountSignInOutValidator validator)
    {
        this._accountService = service;
        this._validator = validator;
    } 

    [HttpPost]
    public async Task<IActionResult> CreateAccountAsync(PostAccountRequest request)
    {
        if(!this._validator.IsValid(request)){ return BadRequest(); }
        string hashedPassword = this._accountService.HashPassword(request.Unhashed_Password);
        
        BaseAccount account;
        if(request.Person_Details is not null){
            account = request.ToPersonAccount(hashedPassword);
        }
        else{
            account = request.ToBusinessAccount(hashedPassword);
        }
    
        var response = await this._accountService.CreateAccountAsync(account);

        if(!response){ return BadRequest(); } // temp

        return Created();
    }

}