using System.Text.Json;
using Dapper;
using Microsoft.VisualBasic;

public class AccountService : IAccountService
{
    private IBlackoutMapConnectionFactory _connectionFactory;
    private JWT_TokenService _tokenService;

    public AccountService(IBlackoutMapConnectionFactory connectionFactory, JWT_TokenService tokenService)
    {
        this._connectionFactory = connectionFactory;
        this._tokenService = tokenService;
    }

    public string HashPassword(string unhashedPassword)
    {
        //<<TODO:to integrate hashing algorithms>>
        return unhashedPassword;
    }

    public async Task<(string, RequestError?)> LoginAccountGetTokenAsync(LoginAccountRequest loginData)
    {
        string token = string.Empty;
        RequestError? error = null;

        string hashedPassword = this.HashPassword(loginData.Password);
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        //<<SQL HERE ! ! ! ! !>>
        SqlMapper.GridReader results = await dbContext.QueryMultipleAsync(
            """
                CREATE TEMP TABLE IF NOT EXISTS user_info AS
                    SELECT Base_Account_id AS user_id FROM Base_Account 
                    WHERE Username = @Username AND Hashed_password = @HashedPassword
                ;

                SELECT user_id FROM user_info LIMIT 1;

                SELECT (EXISTS( 
                    SELECT Person_Account_id FROM Person_Account 
                    WHERE Person_Account_id = ( SELECT user_id FROM user_info LIMIT 1 ) )
                ) AS is_person_account;

                SELECT (EXISTS( 
                    SELECT Business_Account_id FROM Business_Account 
                    WHERE Business_Account_id = ( SELECT user_id FROM user_info LIMIT 1 ) )
                ) AS is_business_account;
            """,
            new {Username = loginData.Username, HashedPassword = hashedPassword}
        );
        
        //verify if it found a Match in the Database
        long? accountId = await results.ReadSingleOrDefaultAsync<long?>();
        if(accountId is null)
        {
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Incorrect Login Data"
            );

            await dbContext.CloseAsync();
            return (token, error);
        }

        bool isPersonAccount = await results.ReadSingleAsync<bool>();
        bool isBusinessAccount = await results.ReadSingleAsync<bool>();

        //Verify possible inconsistency in the Database
        if(isPersonAccount == true && isBusinessAccount == true)
        {
            await dbContext.CloseAsync();
            throw new InvalidOperationException("Server Critical Error," +
            " account is both Person and Business in the Database");
        }

        JWT_AccountData accountData;
        if(isPersonAccount == true){
            accountData = new((int)accountId, typeof(PersonAccount));
        }else{
            accountData = new((int)accountId, typeof(BusinessAccount));   
        }

        await dbContext.CloseAsync();

        token = this._tokenService.CreateToken(accountData);
        return (token, error);
    }

    public async Task<BaseAccount> CreateAccountAsync(BaseAccount baseAccount)
    { 
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        long accountId = await dbContext.QuerySingleAsync<long>(
            """
                INSERT INTO Base_Account (Username, Hashed_password, Email, District_id)
                VALUES (@Username, @Hashed_password, @Email, @District_id)
                RETURNING Base_Account_id;
            """,
            new{ Username = baseAccount.Username, Hashed_password = baseAccount.HashedPassword, 
                 Email = baseAccount.Email, District_id = baseAccount.DistrictId}
        );

        switch(baseAccount)
        {
            case PersonAccount pAccount: 
                pAccount = await dbContext.QuerySingleAsync<PersonAccount>(
                """
                    INSERT INTO Person_Account (Person_Account_id, Birthday)
                    VALUES (@AccountId, @Birthday);

                    SELECT pa.Birthday, 
                           ba.Username, ba.Email, ba.UTC_datetime_creation, 
                           ba.Advertisement_slots_amount, ba.District_id
                    FROM Person_Account AS pa JOIN Base_Account AS ba ON pa.Person_Account_id = ba.Base_Account_id
                    WHERE pa.Person_Account_id = @AccountId;
                """,
                new{AccountId = accountId, 
                    Birthday = JsonSerializer.Serialize(pAccount.Birthday).Replace("\"", string.Empty)}
                );
    
            dbContext.Close();
            return pAccount;

            //----------------------------------------------------------------------------------
            case BusinessAccount bAccount: 
                bAccount = await dbContext.QuerySingleAsync<BusinessAccount>(
                """
                    INSERT INTO Business_Account (Business_Account_id, Cnpj)
                    VALUES (@AccountId, @CNPJ);

                    UPDATE Base_Account SET Advertisement_slots_amount = 1
                    WHERE Base_Account_id = @AccountId;  

                    SELECT busa.Cnpj, 
                           ba.Username, ba.Email, ba.UTC_datetime_creation, 
                           ba.Advertisement_slots_amount, ba.District_id
                    FROM Business_Account AS busa JOIN Base_Account AS ba ON busa.Business_Account_id = ba.Base_Account_id
                    WHERE busa.Business_Account_id = @AccountId;  
                """,
                new{AccountId = accountId, CNPJ = bAccount.CNPJ }
                );
            
            dbContext.Close();
            return bAccount;
        }

        dbContext.Close();
        throw new InvalidDataException("Object is neither Person nor Business Account");
    }

    public async Task<(GetAccountDataResponse, RequestError?)> GetAccountData(int account_id, string accountType, bool includePrivateData)
    {
        // RequestError? error = null;
        PersonAccountData? personData = null;
        BusinessAccountData? businessData = null;

        using var dbcontext = await this._connectionFactory.CreateConnectionAsync();

        switch(accountType)
        {
            case nameof(PersonAccount): 
                PersonAccountData pResult = (await dbcontext.QueryAsync<PersonAccount, string, PersonAccountData>(
                """
                    SELECT 
                        p.Birthday, ba.Username, ba.Email, ba.Description, 
                        ba.Advertisement_slots_amount, ba.District_id, ba.Profile_picture_link,
                        d.District_Name
                    FROM 
                        Base_Account AS ba JOIN
                        Person_Account AS p ON ba.Base_Account_id = p.Person_Account_id JOIN
                        District AS d       ON ba.District_id = d.District_Id
                    WHERE
                        ba.Base_Account_id = @accountId;
                """
                ,
                (person, districtName) => { return person.ToPersonAccountData(districtName, includePrivateData: includePrivateData); }
                ,
                new{ accountId = account_id}
                ,
                splitOn: "District_Name"
                )).First();

                personData = pResult;
            break;
        //--------------------------------------------------------
            case nameof(BusinessAccount):
                BusinessAccountData bResult = (await dbcontext.QueryAsync<BusinessAccount, string, BusinessAccountData>(
                """
                    SELECT 
                        b.Cnpj, ba.Username, ba.Email, ba.Description, 
                        ba.Advertisement_slots_amount, ba.District_id, ba.Profile_picture_link,
                        d.District_Name
                    FROM 
                        Base_Account AS ba JOIN
                        Business_Account AS b ON ba.Base_Account_id = b.Business_Account_id JOIN
                        District AS d       ON ba.District_id = d.District_Id
                    WHERE
                        ba.Base_Account_id = @accountId;
                """
                ,
                (business, districtName) => { return business.ToBusinessAccountData(districtName, includePrivateData: includePrivateData); }
                ,
                new{ accountId = account_id}
                ,
                splitOn: "District_Name"
                )).First();

                businessData = bResult;
            break; 
        }

        //recover public and private ads of the account
        SqlMapper.GridReader results = await dbcontext.QueryMultipleAsync(
            $"""
                SELECT --PUBLIC Ads
                    ad.Advertisement_id     AS {nameof(AdvertisementSummary.ad_Id)},
                    m.Message_text          AS {nameof(AdvertisementSummary.ad_Text)},
                    m.Message_image_link    AS {nameof(AdvertisementSummary.ad_Image_Link)},
                    ad.Redirect_link        AS {nameof(AdvertisementSummary.ad_Redirect_Link)},
                    m.Is_hidden             AS {nameof(AdvertisementSummary.is_Hidden)}
                FROM
                    Advertisement AS ad JOIN
                    Message AS m ON m.Message_id = ad.Message_id
                WHERE
                    m.Is_hidden = FALSE AND --IS HIDDEN set to FALSE
                    m.Base_Account_id = @accountId
                    
                    ;

                SELECT --PRIVATE Ads
                    ad.Advertisement_id     AS {nameof(AdvertisementSummary.ad_Id)},
                    m.Message_text          AS {nameof(AdvertisementSummary.ad_Text)},
                    m.Message_image_link    AS {nameof(AdvertisementSummary.ad_Image_Link)},
                    ad.Redirect_link        AS {nameof(AdvertisementSummary.ad_Redirect_Link)},
                    m.Is_hidden             AS {nameof(AdvertisementSummary.is_Hidden)}
                FROM
                    Advertisement AS ad JOIN
                    Message AS m ON m.Message_id = ad.Message_id
                WHERE
                    m.Is_hidden = TRUE AND --IS HIDDEN set to TRUE
                    m.Base_Account_id = @accountId;
            """,
            new {accountId = account_id}
        );

        var publicAds  = await results.ReadAsync<AdvertisementSummary>() ?? Enumerable.Empty<AdvertisementSummary>();
        var privateAds = await results.ReadAsync<AdvertisementSummary>() ?? Enumerable.Empty<AdvertisementSummary>();

        if(accountType == nameof(PersonAccount)){
            personData!.public_Data.visible_Ads.AddRange(publicAds);
            if(includePrivateData){
                personData!.private_Data!.hidden_Ads.AddRange(privateAds);
            }
        }
        else{
            businessData!.public_Data.visible_Ads.AddRange(publicAds);
            if(includePrivateData){
                businessData!.private_Data!.hidden_Ads.AddRange(privateAds);
            }
        }
        
        await dbcontext.CloseAsync();

        GetAccountDataResponse response = new GetAccountDataResponse(personData, businessData);
        return (response, null);
    }

    public async Task<(Advertisement?, RequestError?)> PostAdvertisementAsync(PostAdvertisementRequest request, int accountId, string accountType)
    {
        RequestError? error = null;
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();
        bool adValidToBoost = nameof(BusinessAccount) == accountType;

        //Verify if it has enough slots available
        int slotsAvailable = (int)await dbContext.QuerySingleAsync<long>(
            """
                SELECT ( 
                	--get total amount of ad slots of the account
                    (SELECT Advertisement_slots_amount FROM Base_Account WHERE Base_Account_id = @accountId ) 
                    
                    --Subtraction operation
                    - 

                    --get all the current published ads of the account
                    (SELECT COALESCE((
                        SELECT COUNT(ad.Advertisement_id)
                        FROM Advertisement    AS ad 
                            JOIN Message      AS m  ON ad.Message_id = m.Message_id
                            JOIN base_Account AS ba ON ba.Base_Account_id = m.Base_Account_id 
                        WHERE ba.Base_Account_id = @accountId
                        GROUP BY ba.Base_Account_id)
                    , 0))
                ) AS Slots_Available;
            """,
            new{ accountId = accountId }
        );

        //Validates the intended action
        if(slotsAvailable <= 0){
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Account already has all Advertisement slots in use."
            );

            await dbContext.CloseAsync();
            return (default, error);
        }

        //Create message, then Advertisement entry in the Database
        var message_ad = await dbContext.QuerySingleAsync<Message>(
            """
                INSERT INTO MESSAGE (Message_text, Is_hidden, Base_Account_Id) 
                VALUES (@messageText, @isHidden, @accountId)
                RETURNING *;
            """,
            new{ messageText = request.ad_Text ?? ".",  isHidden = request.is_Hidden ?? true, 
                 accountId = accountId}
        );

        var advertisement = await dbContext.QuerySingleAsync<Advertisement>(
            """
                INSERT INTO Advertisement (Message_id, Redirect_link, Valid_to_boost)
                VALUES (@messagId, @redirectLink, @ValidBoost)
                RETURNING Advertisement_id, Valid_to_boost, UTC_last_edit, UTC_boost_ends_at, Redirect_link;
            """,
            new {messagId = message_ad.Id, redirectLink = request.redirect_Link, ValidBoost = adValidToBoost}
        );

        //IMPORTANT! compose generated objects
        Advertisement response = new Advertisement(advertisement, message_ad);

        await dbContext.CloseAsync();
        return (response, null);
    }

    public async Task<(PostBoostAdvertisementReponse?, RequestError?)> BoostAdvertisementAsync(int ad_id)
    {
        const int placeholder_DefaultBoostDurationDays = 30;

        RequestError? error = null;
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();
        
        var result = await dbContext.QueryFirstAsync<PostBoostAdvertisementReponse>(
            $"""
                UPDATE Advertisement 
                SET UTC_boost_ends_at = 
                    MAX(UTC_boost_ends_at, unixepoch('now')) --To use the best money-cost time update plan
                    -- PLUS
                    + 
                    ({placeholder_DefaultBoostDurationDays} * 24 * 60 * 60) --days to seconds
                WHERE Advertisement_id = @adId
                RETURNING
                    Advertisement_id  AS {nameof(PostBoostAdvertisementReponse.ad_Id)}
                    ,
                    UTC_boost_ends_at AS {nameof(PostBoostAdvertisementReponse.utc_Boost_Ends_At)}
            """,
            new{adId = ad_id}
        );
        
        await dbContext.CloseAsync();
        return (result, error);
    }


}

public interface IAccountService
{
    public Task<BaseAccount> CreateAccountAsync(BaseAccount baseAccount);
    public Task<(string, RequestError?)> LoginAccountGetTokenAsync(LoginAccountRequest loginData);
    public Task<(GetAccountDataResponse, RequestError?)> GetAccountData(int account_id, string accountType, bool includePrivateData);
    public Task<(Advertisement?, RequestError?)> PostAdvertisementAsync(PostAdvertisementRequest request, int accountId, string accountType);
    public Task<(PostBoostAdvertisementReponse?, RequestError?)> BoostAdvertisementAsync(int ad_id);

    public string HashPassword(string unhashedPassword);
}