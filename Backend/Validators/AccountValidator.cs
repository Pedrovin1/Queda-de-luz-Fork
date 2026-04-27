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

    public async Task<(bool, int?, RequestError?)> AccountExistsAsync(string accountUsername)
    {
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();
        
        int? accountId = await dbContext.QueryFirstOrDefaultAsync<int?>(
            """
                SELECT  Base_Account_id 
                FROM    Base_Account 
                WHERE   Username = @username;
            """,
            new {username = accountUsername}
        );
        await dbContext.CloseAsync();

        if(accountId is null)
        {
            RequestError error = new RequestError(StatusCodes.Status404NotFound, 
            $"User [{accountUsername}] Not Found");
            return (false, null, error);
        }

        return (true, accountId, null);
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

        if(request.username is null || request.username == string.Empty)
        {
            isValid = false;
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "invalid username string format");
            return (isValid, error);
        }
        if(request.password is null || request.password == string.Empty)
        {
            isValid = false;
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "invalid password string format");
            return (isValid, error);
        }

        return (isValid, error);
    }

    public (bool, RequestError?) IsValid(PostAdvertisementRequest request)
    {
        const int Message_Max_Char_Length = 1000;
        RequestError? error = null;

        if(request.ad_Text is not null && request.ad_Text.Length > Message_Max_Char_Length)
        {
            error = new(
                StatusCodes.Status400BadRequest,
                $"text length cannot be bigger than {Message_Max_Char_Length}"
            );
            return (false, error);
        }

        if(request.redirect_Link is not null && request.redirect_Link.Length > Message_Max_Char_Length)
        {
            error = new(
                StatusCodes.Status400BadRequest,
                 $"link length cannot be bigger than {Message_Max_Char_Length}"
            );
            return (false, error);
        }
        
        return (true, error);
    }

    public async Task<(bool, RequestError?)> IsAccountOwnerOfAdAsync(int accountId, int adId)
    {
        RequestError? error = null;
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        bool? result = await dbContext.QueryFirstOrDefaultAsync<bool?>(
            """
                SELECT EXISTS( 
                    SELECT * 
                    FROM Advertisement AS ad
                        JOIN Message AS m ON ad.Message_id = m.Message_id
                    WHERE 
                        ad.Advertisement_id = @adId AND
                        m.Base_Account_id = @accountId
                );
            """,
            new{adId = adId, accountId = accountId}
        );

        await dbContext.CloseAsync();

        // for security reasons, if the ad exists but the user 
        // requesting isnt the owner, they shouldnt know that the Ad even exists
        if(result != true){
            await dbContext.CloseAsync();
            error = new RequestError(StatusCodes.Status404NotFound, 
            $"Advertisement [{adId}] Not Found"
            );
            return (false, error);
        }

        return (true, null);
    }


    public async Task<(bool, RequestError?)> IsAdValidToBoostAsync(int accountId, int adId)
    {
        RequestError? error = null;
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        bool? result = await dbContext.QueryFirstOrDefaultAsync<bool?>(
            """
                SELECT EXISTS( 
                    SELECT * 
                    FROM Advertisement AS ad
                        JOIN Message AS m ON ad.Message_id = m.Message_id
                    WHERE 
                        ad.Advertisement_id = @adId AND
                        m.Base_Account_id = @accountId
                );
            """,
            new{adId = adId, accountId = accountId}
        );

        // for security reasons, if the ad exists but the user 
        // requesting isnt the owner, they shouldnt know that the Ad even exists
        if(result != true){
            await dbContext.CloseAsync();
            error = new RequestError(StatusCodes.Status404NotFound, 
            $"Advertisement [{adId}] Not Found"
            );
            return (false, error);
        }

        result = await dbContext.QueryFirstOrDefaultAsync<bool?>(
            """
                SELECT Valid_to_boost 
                FROM Advertisement 
                WHERE Advertisement_id = @adId;
            """,
            new{adId = adId}
        );

        if(result is null){
            await dbContext.CloseAsync();
            error = new RequestError(
                StatusCodes.Status404NotFound,
                $"Advertisement [{adId}] Not Found"
            );
            return (false, error);
        }

        if(result == false){
            await dbContext.CloseAsync();
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                $"Advertisement [{adId}] Is not allowed to be boosted"
            );
            return (false, error);
        }

        return (true, null);
    }

    public async Task <(bool, RequestError?)> IsValid(PutEditAccountDataRequest request, string accountTypeFromToken, int accountId)
    {
        RequestError? error = null;

        if(request.business_Data is null && request.person_Data is null){
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Invalid Json Schema, Both Person and Business Data are Null"
            );
            return(false, error);
        }

        if(request.business_Data is not null && request.person_Data is not null){
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Invalid Json Schema, Both Person and Business Data are Not Null"
            );
            return(false, error);
        }

        var data = (username:(string?)null, districtId:(int?)null , email:(string?)null);

        string requestAccountType = request.person_Data is not null ? nameof(PersonAccount) 
                                                                    : nameof(BusinessAccount);

        //Check if account is editing using the correct AccountType Schema
        if(requestAccountType != accountTypeFromToken){
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Account type from Json is different than the client account type"
            );
            return(false, error);
        }

        if(requestAccountType == nameof(PersonAccount) ){
            data.username = request.person_Data!.username;
            data.districtId = request.person_Data!.district_Id;
            data.email = request.person_Data.email;
        }else{
            data.username = request.business_Data!.username;
            data.districtId = request.business_Data!.district_Id;
            data.email = request.business_Data.email;
        }

        if(data.email is null){
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Email Cannot be null"
            );
            return(false, error);
        }

        using var dbContext = await this._connectionFactory.CreateConnectionAsync();
        SqlMapper.GridReader results = await dbContext.QueryMultipleAsync(
            """
                SELECT (EXISTS(
                    SELECT * FROM District WHERE District_id = @districtId
                )) AS District_Exists;

                SELECT (NOT EXISTS(
                    SELECT * FROM Base_Account 
                    WHERE 
                        Username = @username
                    AND Base_Account_id != @accountId
                )) AS Username_available;
            """,
            new{ districtId = (int)data.districtId, username = (string)data.username, accountId = accountId}
        );
        
        bool requestIntegrityStatus = true;
        //Validate District Id Existence
        requestIntegrityStatus = await results.ReadSingleAsync<bool>();
        if(requestIntegrityStatus == false)
        {
            await dbContext.CloseAsync();
            error = new RequestError(StatusCodes.Status400BadRequest,
            "District does NOT exists");
            return (false, error);
        }

        //Verify Username availability
        requestIntegrityStatus = await results.ReadSingleAsync<bool>();
        if(requestIntegrityStatus == false)
        {
            await dbContext.CloseAsync();
            error = new RequestError(StatusCodes.Status400BadRequest,
            "Username Already Taken");
            return (false, error);
        }
      
        await dbContext.CloseAsync();
        return (requestIntegrityStatus, error);
    }
}