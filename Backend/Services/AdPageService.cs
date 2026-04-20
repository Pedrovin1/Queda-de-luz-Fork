using Dapper;

public class AdPageService : IAdPageService
{
    private IBlackoutMapConnectionFactory _connectionFactory;

    public AdPageService(IBlackoutMapConnectionFactory connFactory)
    {
        this._connectionFactory = connFactory;
    }

    public async Task< List<ActiveAds> > GetActiveAdsAsync(GetActiveAdsRequest_QueryParams queryParams)
    {
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        var activeAds = await dbContext.QueryAsync<ActiveAds>(
            $"""
                SELECT  ad.Advertisement_id AS      {nameof(ActiveAds.ad_Id)}, 
                        m.Message_text AS           {nameof(ActiveAds.ad_Text)}, 
                        m.Message_image_link AS     {nameof(ActiveAds.ad_Image_Link)}, 
                        ad.Redirect_link AS         {nameof(ActiveAds.ad_Redirect_Link)}, 
                        ba.Base_Account_id AS       {nameof(ActiveAds.account_Owner_Id)}, 
                        ba.Username AS              {nameof(ActiveAds.account_Owner_Username)}
                FROM Advertisement AS ad 
                    JOIN Message As m ON ad.Message_id = m.Message_id
                    JOIN Base_Account AS ba ON m.Base_Account_id = ba.Base_Account_id
                WHERE 
                    ad.UTC_boost_ends_at > unixepoch('now') AND 
                    m.Is_hidden = FALSE AND
                    ba.District_id = @districtId;
            """,
            new{ districtId = queryParams.district_Id }
        );

        await dbContext.CloseAsync();
        return activeAds is null ? new() : activeAds.ToList();
    }
    
}

public interface IAdPageService
{
    public Task< List<ActiveAds> > GetActiveAdsAsync(GetActiveAdsRequest_QueryParams queryParams);
}