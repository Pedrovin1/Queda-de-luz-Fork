public static class MessageMapping
{
    public static Message ToMessage(this PostMessageRequest request, int userId)
    {
        return new Message(
            Message_text: request.Message_Text,
            Base_Account_id: userId
        );
    }

    public static PostMessageResponse ToPostMessageResponse(this Message message)
    {
        return new PostMessageResponse(
            Message_Id: (int)message.Id!,
            Text: message.Text,
            Image_Link: message.ImageLink!,
            utc_Time_Sent: (long)message.UTC_TimeSent!,
            Account_Id: message.AccountId,
            Is_Hidden: message.IsHidden
        );
    }
}