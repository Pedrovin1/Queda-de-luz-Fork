
using System.Threading.Tasks;
using Dapper;

public class AutoUpdateRecentReportsService : IHostedService, IDisposable
{
    private Timer? _timer;
    private IBlackoutMapConnectionFactory _connectionFactory;
    private const int defaultIntervalTimeHours = 24;
    private const int defaultReportExpireTimeHours = 48;
    private const float IsFixedRatioThreshold = 25.0f;

    public static string SQL_DeleteRecentReportsWithIsFixedRatio {get; private set; } = 
    $"""
                --Steps to:
                --DELETE a whole group of reports if theres enough positive related reports 
                
                --Positive and Negative (separately) reports of each type (Unique district + problem category + IsFixed)
                CREATE TEMP TABLE IF NOT EXISTS reports_Summary AS
                    SELECT r.Reported_District_id, r.Problem_Category_id, r.Is_Fixed, 
                        COUNT(r.Reported_District_id) AS reportAmount
                    FROM Recent_Report AS recr
                        JOIN Report AS r ON r.Report_id = recr.Report_id
                    GROUP BY r.Problem_Category_id, r.Reported_District_id, r.Is_Fixed 
                ;
        
                --Total SUM of reports (positives + negative) of each type (unique district + problem category)
                CREATE TEMP TABLE IF NOT EXISTS reports_Total_Sum AS
                    SELECT Reported_District_id, Problem_Category_id, SUM(reportAmount) AS  totalReports 
                    FROM reports_Summary
                    GROUP BY Problem_Category_id,Reported_District_id;
                ;
                
                --Percentage of positive reports within their group (unique district + problem category)
                CREATE TEMP TABLE IF NOT EXISTS Reports_Is_Fixed_Ratio AS
                SELECT rsy.Reported_District_id, rsy.Problem_Category_id, rsy.Is_Fixed, rsy.reportAmount, rsm.totalReports,
                    ( CAST(rsy.reportAmount AS REAL ) / rsm.totalReports * 100 ) AS Is_Fixed_Ratio
                FROM reports_Summary AS rsy JOIN reports_Total_Sum AS rsm  
                    ON rsy.Reported_District_id = rsm.Reported_District_id 
                    AND rsy.Problem_Category_id = rsm.Problem_Category_id
                WHERE rsy.Is_Fixed = 1
                ;
                
                WITH recent_district_category_to_delete AS
                (
                    SELECT Reported_District_id, Problem_Category_id 
                    FROM Reports_Is_Fixed_Ratio
                    WHERE Is_Fixed_Ratio >= {IsFixedRatioThreshold} --arbitrary percentage threshold
                )
                --Finally DELETE a whole group of reports if theres enough positive related reports
                DELETE FROM Recent_Report AS rr WHERE
                EXISTS ( 
                    SELECT * 
                    FROM Report AS rep 
                        JOIN recent_district_category_to_delete AS rdcd ON 
                            rep.Reported_District_id = rdcd.Reported_District_id AND
                            rep.Problem_Category_id = rdcd.Problem_Category_id
                        WHERE rr.Report_id = rep.Report_id
                );
    
    """;

    public AutoUpdateRecentReportsService(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this._timer = new Timer(UpdateRecentReports, null, TimeSpan.Zero, TimeSpan.FromHours(defaultIntervalTimeHours));
        return Task.CompletedTask;
    }

    private async void UpdateRecentReports(object? _)
    {
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        await dbContext.ExecuteAsync(
            $"""
                --DELETE expired reports
                DELETE FROM Recent_Report AS rr WHERE EXISTS(
                    SELECT * FROM Report AS r 
                    WHERE 
                        r.Report_id = rr.Report_id 
                    AND
                        (CAST( unixepoch('now') AS INTEGER ) - r.UTC_Date_Report) >= ( {defaultReportExpireTimeHours} * 60 * 60 ) --arbitrary hours value * 60 * 60
                    
                );
                --Steps to:
                --DELETE a whole group of reports if theres enough positive related reports 
                
                --Positive and Negative (separately) reports of each type (Unique district + problem category + IsFixed)
                CREATE TEMP TABLE IF NOT EXISTS reports_Summary AS
                    SELECT r.Reported_District_id, r.Problem_Category_id, r.Is_Fixed, 
                        COUNT(r.Reported_District_id) AS reportAmount
                    FROM Recent_Report AS recr
                        JOIN Report AS r ON r.Report_id = recr.Report_id
                    GROUP BY r.Problem_Category_id, r.Reported_District_id, r.Is_Fixed 
                ;
        
                --Total SUM of reports (positives + negative) of each type (unique district + problem category)
                CREATE TEMP TABLE IF NOT EXISTS reports_Total_Sum AS
                    SELECT Reported_District_id, Problem_Category_id, SUM(reportAmount) AS  totalReports 
                    FROM reports_Summary
                    GROUP BY Problem_Category_id,Reported_District_id;
                ;
                
                --Percentage of positive reports within their group (unique district + problem category)
                CREATE TEMP TABLE IF NOT EXISTS Reports_Is_Fixed_Ratio AS
                SELECT rsy.Reported_District_id, rsy.Problem_Category_id, rsy.Is_Fixed, rsy.reportAmount, rsm.totalReports,
                    ( CAST(rsy.reportAmount AS REAL ) / rsm.totalReports * 100 ) AS Is_Fixed_Ratio
                FROM reports_Summary AS rsy JOIN reports_Total_Sum AS rsm  
                    ON rsy.Reported_District_id = rsm.Reported_District_id 
                    AND rsy.Problem_Category_id = rsm.Problem_Category_id
                WHERE rsy.Is_Fixed = 1
                ;
                
                WITH recent_district_category_to_delete AS
                (
                    SELECT Reported_District_id, Problem_Category_id 
                    FROM Reports_Is_Fixed_Ratio
                    WHERE Is_Fixed_Ratio >= {IsFixedRatioThreshold} --arbitrary percentage threshold
                )
                --Finally DELETE a whole group of reports if theres enough positive related reports
                DELETE FROM Recent_Report AS rr WHERE
                EXISTS ( 
                    SELECT * 
                    FROM Report AS rep 
                        JOIN recent_district_category_to_delete AS rdcd ON 
                            rep.Reported_District_id = rdcd.Reported_District_id AND
                            rep.Problem_Category_id = rdcd.Problem_Category_id
                        WHERE rr.Report_id = rep.Report_id
                );
            """
        );

        await dbContext.CloseAsync();
    }



    public Task StopAsync(CancellationToken cancellationToken)
    {
        this._timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

     public void Dispose()
    {
        this._timer?.Dispose();
    }
}