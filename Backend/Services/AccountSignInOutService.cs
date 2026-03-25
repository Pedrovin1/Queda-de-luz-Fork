using System.Text.Json;
using Dapper;

public class AccountSignInOutService : IAccountSignInOutService
{
    private IBlackoutMapConnectionFactory _connectionFactory;

    public AccountSignInOutService(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }

    public string HashPassword(string unhashedPassword)
    {
        //<<TODO:to integrate hashing algorithms>>
        return unhashedPassword;
    }


    //temporary bool return type
    public async Task<bool> CreateAccountAsync(BaseAccount baseAccount)
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
                await dbcontext.ExecuteAsync(
                """
                    INSERT INTO Person_Account (Person_Account_id, Birthday)
                    VALUES (@AccountId, @Birthday);
                """,
                new{AccountId = accountId, 
                    Birthday = JsonSerializer.Serialize(pAccount.Birthday).Replace("\"", string.Empty)}
                );
            
            dbcontext.Close();
            return true;
            //break;
            case BusinessAccount bAccount: 
                await dbcontext.ExecuteAsync(
                """
                    INSERT INTO Business_Account (Business_Account_id, Cnpj)
                    VALUES (@AccountId, '@CNPJ');

                    UPDATE Base_Account SET Advertisement_slots_amount = 1
                    WHERE Base_Account_id = @AccountId;    
                """,
                new{AccountId = accountId, CNPJ = bAccount.CNPJ }
                );
            
            dbcontext.Close();
            return true;
            //break;
        }

        dbcontext.Close();

        return false;
    }
}

public interface IAccountSignInOutService
{
    public Task<bool> CreateAccountAsync(BaseAccount baseAccount);
    public string HashPassword(string unhashedPassword);
}