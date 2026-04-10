public record PostMessageResponse(
    int Message_Id,
    string Text,
    string Image_Link,
    long utc_Time_Sent,
    int Account_Id,
    bool Is_Hidden
);