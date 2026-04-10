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
                    Profile_Picture_Link: account.ProfilePictureLink)
            );
        }

        var response = new GetChatMembersResponse(membersSummary);
        return(response, null);
    }

    public async Task<(GetRecentMessagesResponse?, RequestError?)> GetRecentMessagesAsync(int chatId)
    {
        const int Message_MAX_Amount = 40;

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

        var recentMessages = await dbContext.QueryAsync<RecentMessages>(
            $"""
                SELECT 
                    m.UTC_datetime_sent  AS {nameof(RecentMessages.utc_Time_Sent)},
                    m.Base_Account_id    AS {nameof(RecentMessages.User_Id)},
                    ba.Username          AS {nameof(RecentMessages.Username)},
                    m.Message_text       AS {nameof(RecentMessages.Message_Text)},
                    m.Message_image_link AS {nameof(RecentMessages.Message_Image_Link)}
                FROM Chat_has_message AS chm 
                    JOIN Message AS m       ON chm.Message_id = m.Message_id
                    JOIN Base_Account AS ba ON m.Base_Account_id = ba.Base_Account_id
                WHERE chm.Chat_id = @ChatId 
                ORDER BY m.UTC_datetime_sent DESC
                LIMIT @MaxMessageAmount;
            """,
            new{ChatId = chatId, MaxMessageAmount = Message_MAX_Amount}
        );

        await dbContext.CloseAsync();

        GetRecentMessagesResponse? response = new(
            Chat_Id: chatId,
            Messages: recentMessages is not null ? recentMessages.ToList() 
                                                 : new List<RecentMessages>()
        );

        return (response, null);
    }

    public async Task<Message> PostMessageAsync(Message messageToCreate, int chatId)
    {
        using var dbContext = await this._connectionFactory.CreateConnectionAsync();
        
        var createdMessage = await dbContext.QuerySingleAsync<Message>(
            """
                INSERT INTO MESSAGE (Message_text, Base_Account_Id) 
                VALUES (@MessageText, @AccountId)
                RETURNING *;
            """,
            new{ MessageText = messageToCreate.Text, AccountId = messageToCreate.AccountId }
        );

        await dbContext.ExecuteAsync(
            """
                INSERT INTO Chat_has_message (Chat_Id, Message_Id)
                VALUES (@ChatId, @MessageId);
            """,
            new{ChatId = chatId, MessageId = createdMessage.Id }
        );
        
        await dbContext.CloseAsync();

        return createdMessage;
    }
}

public interface IChatService
{
    public Task<Message> PostMessageAsync(Message messageToCreate, int chatId);
    public Task<(GetRecentMessagesResponse?, RequestError?)> GetRecentMessagesAsync(int chatId);
    public Task<(GetChatMembersResponse?, RequestError?)> GetChatMembersAsync(int chatId);
}