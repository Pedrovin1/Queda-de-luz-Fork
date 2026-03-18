using System.Data.SQLite;
using Dapper;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

public class HomePageService : IHomePageService
{
    private IBlackoutMapConnectionFactory _dbConnectionFactory;
    public HomePageService(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._dbConnectionFactory = connectionFactory;
    }

    public async Task<List<District>> GetDistrictsAsync(int City_id)
    {
        using SQLiteConnection dbContext = await this._dbConnectionFactory.CreateConnectionAsync();

        City? relatedCity = null;

        var result = await dbContext.QueryAsync<District, City, District>(
            """
                SELECT 
                    d.District_id, d.District_Name,
                    c.City_id, c.City_Name, c.State_Abbreviation
                FROM District AS d 
                    INNER JOIN City AS c ON d.City_id = c.City_id
                WHERE
                    c.City_id = @City_id_
            """,
            (district, city) =>
            {
                relatedCity = relatedCity ?? city; //to avoid unnecessary memory usage, since all city data is the same
                district.City = relatedCity;
                return district;
            },
            new { City_id_ = City_id }
            ,
            splitOn: "City_id"
        );

        await dbContext.CloseAsync();
        
        return result.ToList();
    }

    public async Task<List<long>?> GetCidadeAsync(int id)
    {
        using var dbContext = await this._dbConnectionFactory.CreateConnectionAsync();

        var result = await dbContext.QueryAsync<long>(
            """
            SELECT Cidade_Id
            FROM Cidade
            """
        );

        await dbContext.CloseAsync();

        return result.ToList();
    }
}

public interface IHomePageService
{
    public Task<List<District>> GetDistrictsAsync(int city_id);
    public Task<List<long>?> GetCidadeAsync(int id);
}