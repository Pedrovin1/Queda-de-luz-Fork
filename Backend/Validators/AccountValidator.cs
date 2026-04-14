using Dapper;
using Microsoft.AspNetCore.Http.Features;

public  class AccountValidator
{
    private IBlackoutMapConnectionFactory _connectionFactory;

    public AccountValidator(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }

    public async Task<(bool, string?, RequestError?)> AccountExistsAsync(int accountId)
    {
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();
        
        SqlMapper.GridReader results = await dbContext.QueryMultipleAsync(
            """
                SELECT (EXISTS(
                    SELECT * FROM Person_account AS p WHERE p.Person_account_id = @accountId
                )) AS is_person_account;

                SELECT (EXISTS(
                    SELECT * FROM Business_account AS b WHERE b.Business_account_id = @accountId
                )) AS is_business_account;
            """,
            new {accountId = accountId}
        );

        bool isPersonAccount = await results.ReadSingleAsync<bool>();
        if(isPersonAccount == true)
        {
            await dbContext.CloseAsync();
            string accountType = nameof(PersonAccount);
            return (isPersonAccount, accountType, null);
        }

        bool isBusinessAccount = await results.ReadSingleAsync<bool>();
        if(isBusinessAccount == true)
        {
            await dbContext.CloseAsync();
            string accountType = nameof(BusinessAccount);
            return (isBusinessAccount, accountType, null);
        }
        
        await dbContext.CloseAsync();

        RequestError error = new RequestError(StatusCodes.Status404NotFound, 
        $"User [{accountId}] Not Found");

        return (false, null, error);
    }

    public async Task<(bool, RequestError?)> IsValid(PostAccountRequest request)
    {
        RequestError? error = null;

        //<<TODO: validate Email>>

        //Validate District Id Existence
        //Verify Username availability

        if(request.Person_Details is not null && request.Business_Details is not null ||
           request.Person_Details is null     && request.Business_Details is null)
        {
            error = new RequestError(StatusCodes.Status400BadRequest, 
            "Either PersonDetails or BusinessDetails (Exclusive) should be not null");
            return (false, error);
        }

        //potentially turn into separate funtion
        if(request.Unhashed_Password.Length <= 5 || request.Unhashed_Password.Length > 128)
        {
            error = new RequestError(StatusCodes.Status400BadRequest, 
            "Password does NOT meet length requirements");
            return (false, error);
        }

        using var dbContext = await this._connectionFactory.CreateConnectionAsync();
        SqlMapper.GridReader results = await dbContext.QueryMultipleAsync(
            """
                SELECT (EXISTS(
                    SELECT * FROM District WHERE District_id = @districtId
                )) AS District_Exists;

                SELECT (NOT EXISTS(
                    SELECT * FROM Base_Account WHERE Username = @username
                )) AS Username_available;
            """,
            new{ districtId = request.District_Id, username = request.Username}
        );

        bool requestIntegrityStatus = true;
        //Validate District Id Existence
        requestIntegrityStatus = await results.ReadSingleAsync<bool>();
        if(requestIntegrityStatus == false)
        {
            error = new RequestError(StatusCodes.Status400BadRequest,
            "District does NOT exists");
            return (false, error);
        }

        //Verify Username availability
        requestIntegrityStatus = await results.ReadSingleAsync<bool>();
        if(requestIntegrityStatus == false)
        {
            error = new RequestError(StatusCodes.Status400BadRequest,
            "Username Already Taken");
            return (false, error);
        }
      
        await dbContext.CloseAsync();
        return (requestIntegrityStatus, error);
    }

    public (bool, RequestError?) IsValid(LoginAccountRequest request)
    {
        bool isValid = true; 
        RequestError? error = null;

        if(request.Username is null || request.Username == string.Empty)
        {
            isValid = false;
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "invalid username string format");
            return (isValid, error);
        }
        if(request.Password is null || request.Password == string.Empty)
        {
            isValid = false;
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "invalid password string format");
            return (isValid, error);
        }

        return (isValid, error);
    }
}