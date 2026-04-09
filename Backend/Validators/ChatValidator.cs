using System.Data.Common;
using System.Linq.Expressions;
using Dapper;

public class ChatValidator
{
    private IBlackoutMapConnectionFactory _connectionFactory;

    public ChatValidator(IBlackoutMapConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }

    public async Task<(bool, RequestError?)> IsValid(PostMessageRequest request, int chatId)
    {
        const int Message_Max_Char_Size = 1000;
        RequestError? error = null;

        using var dbContext = await this._connectionFactory.CreateConnectionAsync();

        bool chatExists = await dbContext.QuerySingleOrDefaultAsync<bool>(
            """
                SELECT (EXISTS(
                    SELECT * FROM Chat WHERE Chat_id = @ChatId
                )) AS chat_exists;
            """,
            new{ChatId = chatId}
        );
        await dbContext.CloseAsync();

        //chat doesnt exist
        if(chatExists == false)
        {
            error = new RequestError(
                StatusCodes.Status404NotFound,
                $"Chat [{chatId}] Not Found"
            );
            return (false, error);
        }

        //empty message
        if(string.IsNullOrWhiteSpace(request.Message_text))
        {
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Empty Message not Valid"
            );
            return (false, error);
        }
        
        //message too large
        if(request.Message_text.Length > Message_Max_Char_Size)
        {
            error = new RequestError(
                StatusCodes.Status400BadRequest,
                "Message Too Large"
            );
            return (false, error);
        }
        
        return (true, null);
    }
}