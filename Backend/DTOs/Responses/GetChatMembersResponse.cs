public record GetChatMembersResponse(
    List<ChatMemberSummary> Members
);

public record ChatMemberSummary(
    int Id,
    string Username,
    string? ProfilePictureLink
);