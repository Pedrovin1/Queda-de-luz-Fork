using Dapper;

public class ChatService : IChatService
{
    private IBlackoutMapConnectionFactory _connectionFactory;

    public ChatService(IBlackoutMapConnectionFactory connFactory)
    {
        this._connectionFactory = connFactory;
    }

    public async Task<(GetChatMembersResponse?, RequestError?)> GetChatMembersAsync(int chatId)
    {
        RequestError? error = null;
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        bool chatExists = await dbContext.QuerySingleOrDefaultAsync<bool>(
            """
                SELECT (EXISTS(
                    SELECT * FROM Chat 
                    WHERE Chat_id = @chatId)
                ) AS chat_exists;
            """,
            new{chatId = chatId}
        );

        if(chatExists == false)
        {
            error = new RequestError(
                StatusCodes.Status404NotFound,
                $"Chat [{chatId}] Not Found"
            );

            await dbContext.CloseAsync();
            return (default, error);
        }

        var baseAccountsResult = await dbContext.QueryAsync<BaseAccount>(
            """
                WITH ChatDistrictId AS (
                    SELECT District_id FROM Chat 
                    WHERE Chat_id = @chatId 
                    LIMIT 1
                )

                SELECT ba.Base_Account_id, ba.Username, ba.Profile_picture_link
                FROM Base_Account AS ba 
                WHERE ba.District_id = (SELECT District_id FROM ChatDistrictId LIMIT 1);
            """,
            new{chatId = chatId}
        );

        await dbContext.CloseAsync();

        List<ChatMemberSummary> membersSummary = new();
        foreach(BaseAccount account in baseAccountsResult)
        {
            membersSummary.Add(
                new ChatMemberSummary(
                    Id:account.Id,
                    Username: account.Username,
                    ProfilePictureLink: account.ProfilePictureLink)
            );
        }

        var response = new GetChatMembersResponse(membersSummary);
        return(response, null);
    }

    public async Task<object> GetRecentMessagesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<object> PostMessageAsync()
    {
        throw new NotImplementedException();
    }
}

public interface IChatService
{
    public Task<object> PostMessageAsync();
    public Task<object> GetRecentMessagesAsync();
    public Task<(GetChatMembersResponse?, RequestError?)> GetChatMembersAsync(int chatId);
}