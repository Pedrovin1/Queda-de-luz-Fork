using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

[ApiController]
[Route("chats")]
public class ChatController : ControllerBase
{
    private IChatService _chatService;
    private ChatValidator _validator;

    public ChatController(IChatService service, ChatValidator validator)
    {
        this._chatService = service;
        this._validator = validator;
    }

    public Task<IActionResult> PostMessageAsyc()
    {
        return default;
    } 
    public Task<IActionResult> GetRecentMessagesAsyc()
    {
        return default;
    } 
    
    [HttpGet]
    [Route("{chat_id}/members")]
    public async Task<IActionResult> GetChatMembersAsync(int chat_id)
    {
        (GetChatMembersResponse? response, var error) = await this._chatService.GetChatMembersAsync(chat_id);

        if(error is not null)
        {
            return this.StatusCode(error.StatusCode, error.Message);
        }

        return Ok(response);
    }
}