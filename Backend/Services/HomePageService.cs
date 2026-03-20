using System.Data.SQLite;
using Dapper;

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

    public async Task<GetCityStatisticsResponse> GetCityStatistics(int city_id)
    {
        using var dbContext = await this._dbConnectionFactory.CreateConnectionAsync();

        string city_Name=string.Empty;
        Dictionary<int, DistrictStatistics> result = new();

        _ = await dbContext.QueryAsync<string, District, ProblemCategory, long, bool>(
            """
            SELECT c.City_Name, 
                dis.District_id, dis.District_Name, 
                pc.Problem_Category_id, pc.Problem_Category_Name, 
                COUNT(rep.Problem_Category_id) AS Reports_Amount
            FROM Recent_Report AS recrep
                JOIN Report           AS rep ON recrep.Report_id = rep.Report_id
                JOIN District         AS dis ON rep.Reported_District_id = dis.District_id
                JOIN City             AS c   ON dis.City_id = c.City_id
                JOIN Problem_Category AS pc  ON rep.Problem_Category_id = pc.Problem_Category_id
            WHERE rep.Is_Fixed = FALSE AND c.City_id = @CityId --queried City_id
            GROUP BY dis.District_id, pc.Problem_Category_id;
            """,

            (cityName, district, problemCategory, problemAmount) => 
            {
                city_Name = city_Name == string.Empty ? cityName : city_Name;
                if(!result.ContainsKey(district.Id)){result.Add(district.Id, 
                                                                new DistrictStatistics(district.Name, new List<DistrictSingleStatistic>()));}
                
                result[district.Id].District_Statistics.Add(new DistrictSingleStatistic(
                                                                    problemCategory.Id, problemCategory.Name, 
                                                                    (int)problemAmount));
                return true;
            }
            
            ,
            new{ CityId = city_id }
            ,
            splitOn: "District_id,Problem_Category_id,Reports_Amount"
        );

        await dbContext.CloseAsync();

        return new GetCityStatisticsResponse(city_Name, result);
    }

    public async Task<GetCitiesResponse?> GetCitiesAsync(string state_abbreviation)
    {
        using var dbContext = await this._dbConnectionFactory.CreateConnectionAsync();

        var result = await dbContext.QueryAsync<City>(
            $"""
            SELECT City_id, City_Name 
            FROM City 
            WHERE State_Abbreviation = '{state_abbreviation}'; 
            """
            //,
            //new{ stateAbbreviation = state_abbreviation}
        );

        await dbContext.CloseAsync();

        GetCitiesResponse? response = new(state_abbreviation, new());
        foreach(City c in result)
        {
            response.Cities.Add( new GetCitiesResponse_City(c.Id, c.Name) );
            //response.Cities.Add( (c.Id, c.Name) );
        }

        if(response.Cities is null || response.Cities.Count <= 0){ response = null; }

        return response;
    }

}

public interface IHomePageService
{
    public Task<List<District>> GetDistrictsAsync(int city_id);
    public Task<Report> PostReportAsync(Report report);
    public Task<GetCityStatisticsResponse> GetCityStatistics(int city_id);
    public Task<GetCitiesResponse?> GetCitiesAsync(string state_abbreviation);
}