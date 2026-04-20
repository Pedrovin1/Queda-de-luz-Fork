using Dapper;

public  class AdPageValidator
{
    private IBlackoutMapConnectionFactory _connectionFactory;
    public AdPageValidator(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }

    public async Task<(bool, RequestError?)> IsValid(GetActiveAdsRequest_QueryParams requestParams)
    {
        RequestError? error = null;
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        if(requestParams.district_Id is null)
        {
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "district_Id query Param cannot be Null"
            );
            return (false, error);
        }

        bool districtExists = await dbContext.QueryFirstAsync<bool>(
            """
                SELECT (EXISTS(
                    SELECT * FROM District WHERE District_id = @districtId
                )) AS District_Exists;
            """,
            new{districtId = requestParams.district_Id}
        );

        await dbContext.CloseAsync();

        if(districtExists == false)
        {
            error = new RequestError(
                StatusCodes.Status404NotFound,
                $"district [{requestParams.district_Id}] Not Found"
            );
            return (false, error);
        }

        return (true, null);
    } 
}