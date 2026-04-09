public record PostMessageResponse(
    int Id,
    string Text,
    string Image_Link,
    long UTC_Time_Sent,
    int Account_Id,
    bool Is_Hidden
);