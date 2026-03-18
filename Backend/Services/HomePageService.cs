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

    public async Task<Report> PostReportAsync(Report report)
    {
        using var dbContext = await this._dbConnectionFactory.CreateConnectionAsync();

        Report result = await dbContext.QuerySingleAsync<Report>(
            $"""
            INSERT INTO Report (Problem_Category_id, Reported_District_id, Base_Account_id) 
            VALUES (@ProblemCategory_Id, @District_Id, @AccountId) 
            RETURNING *;
            """,
            new{ ProblemCategory_Id = report.ProblemCategoryId, 
                 District_Id = report.ReportedDistrictId,
                 AccountId = report.AccountId is not null ? report.AccountId : null}
        );

        await dbContext.CloseAsync();

        return result;
    }
}

public interface IHomePageService
{
    public Task<List<District>> GetDistrictsAsync(int city_id);
    public Task<Report> PostReportAsync(Report report);
}