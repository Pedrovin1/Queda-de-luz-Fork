using Dapper;

public class HomePageValidator
{
    private IBlackoutMapConnectionFactory _connectionFactory;

    public HomePageValidator(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }

    public async Task<(bool, RequestError?)> IsValid(PostReportRequest request, int cityId, int districtId)
    {
        RequestError? error = null;
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        //Check ProblemId existence
        //Check connection between City and District
        //If IsFixed=true, verify if is there at least one IsFixed=false, otherwise bad request

        string extraQuery = string.Empty;
        if(request.Is_Fixed == true)
        {
            extraQuery =
            """
                SELECT (EXISTS(
                    SELECT *
                    FROM Recent_Report AS rr JOIN Report AS rep ON rr.Report_id = rep.Report_id
                    WHERE rep.Is_fixed = FALSE AND rep.Problem_Category_id = @problemId AND rep.Reported_District_id = @districtId)
                ) AS Negative_report_exists;
            """;
        }

        using SqlMapper.GridReader? results = await dbContext.QueryMultipleAsync(
            $"""
                SELECT (EXISTS(
                    SELECT * FROM Problem_Category WHERE Problem_Category_id = @problemId )
                ) AS Problem_exists;

                SELECT (EXISTS(
                    SELECT * FROM District WHERE City_id = @cityId AND District_id = @districtId)
                ) AS Is_district_inside_city;

                {extraQuery}
            """,
            new{problemId = request.Problem_Category_id, 
                cityId = cityId, 
                districtId = districtId}
        );

        //<<TODO:potentially changeable to a while(results.IsConsumed == false)>>

        bool requestIntegrityStatus = true;
        //Check ProblemId existence
        requestIntegrityStatus = await results.ReadSingleAsync<bool>();
        if(requestIntegrityStatus == false)
        {
            error = new RequestError(StatusCodes.Status400BadRequest, 
            "Problem_Category Does NOT Exist");
            await dbContext.CloseAsync();
            return (requestIntegrityStatus, error);
        }

        //Check connection between City and District
        requestIntegrityStatus = await results.ReadSingleAsync<bool>();
        if(requestIntegrityStatus == false)
        {
            error = new RequestError(StatusCodes.Status400BadRequest, 
            "City does NOT contain the specified District");
            await dbContext.CloseAsync();
            return (requestIntegrityStatus, error);
        }

        //If IsFixed=true, verify if there is at least one IsFixed=false
        //it doesnt make sense to report a problem solved for something that isnt broken in the first place
        if(request.Is_Fixed == true)
        {
            requestIntegrityStatus = await results.ReadSingleAsync<bool>();  
            if(requestIntegrityStatus == false)
            {
                error = new RequestError(StatusCodes.Status400BadRequest, 
                "There are no Negative Reports of this type currently registered");
                await dbContext.CloseAsync();
                return (requestIntegrityStatus, error);
            } 
        }
        
        await dbContext.CloseAsync();
        return (requestIntegrityStatus, error);
    }
}