using System.Data.SQLite;
using System.Runtime.CompilerServices;
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

        //Create a report in the database
        Report result = await dbContext.QuerySingleAsync<Report>(
            """
            INSERT INTO Report (Is_Fixed, Problem_Category_id, Reported_District_id, Base_Account_id) 
            VALUES (@IsFixed, @ProblemCategory_Id, @District_Id, @AccountId) 
            RETURNING *;
            """,
            new{ IsFixed = report.IsFixed,
                 ProblemCategory_Id = report.ProblemCategoryId, 
                 District_Id = report.ReportedDistrictId,
                 AccountId = report.AccountId is not null ? report.AccountId : null}
        );

        //"add" the recently created report to the recent reports table
        await dbContext.ExecuteAsync(
            """
            INSERT INTO Recent_Report (Report_id) VALUES (@reportId);
            """,
            new{ reportId = result.Id }
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