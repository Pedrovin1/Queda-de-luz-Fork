using System.Text.Json;
using Dapper;

public class AccountSignInOutService : IAccountSignInOutService
{
    private IBlackoutMapConnectionFactory _connectionFactory;
    private JWT_TokenService _tokenService;

    public AccountSignInOutService(IBlackoutMapConnectionFactory connectionFactory, JWT_TokenService tokenService)
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
        using var dbcontext = await this._connectionFactory.CreateConnectionAsync();

        long accountId = await dbcontext.QuerySingleAsync<long>(
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
                pAccount = await dbcontext.QuerySingleAsync<PersonAccount>(
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
    
            dbcontext.Close();
            return pAccount;

            //----------------------------------------------------------------------------------
            case BusinessAccount bAccount: 
                bAccount = await dbcontext.QuerySingleAsync<BusinessAccount>(
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
            
            dbcontext.Close();
            return bAccount;
        }

        dbcontext.Close();
        throw new InvalidDataException("Object is neither Person nor Business Account");
    }
}

public interface IAccountSignInOutService
{
    public Task<BaseAccount> CreateAccountAsync(BaseAccount baseAccount);
    public Task<(string, RequestError?)> LoginAccountGetTokenAsync(LoginAccountRequest loginData);
    public string HashPassword(string unhashedPassword);
}