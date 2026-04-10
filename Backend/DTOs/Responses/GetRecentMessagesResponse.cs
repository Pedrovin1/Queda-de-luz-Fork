public record GetRecentMessagesResponse(
    int Chat_Id,
    List<RecentMessages> Messages
);

public record RecentMessages(
    long utc_Time_Sent,
    long User_Id,
    string Username,
    string Message_Text,
    string? Message_Image_Link = null
);