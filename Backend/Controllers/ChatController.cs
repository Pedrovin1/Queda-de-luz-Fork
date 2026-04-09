using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    // [RequestSizeLimit(52428800)] //<<TODO: to implement image file handling safeguards>>
    [HttpPost]
    [Authorize]
    [Route("{chat_id}/messages")]
    public async Task<IActionResult> PostMessageAsync(int chat_id, PostMessageRequest request)
    {
        (bool isValid, RequestError? error) = await this._validator.IsValid(request, chat_id);
        if(isValid == false)
        {
            return this.StatusCode(error!.StatusCode, error.Message);
        }

        string? textUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? null;
        if(textUserId is null)
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Invalid token format");
        }
        int parsedId = int.Parse(textUserId);

        Message message = request.ToMessage(parsedId);
        Message createdMessage = await this._chatService.PostMessageAsync(message, chat_id);

        var response = createdMessage.ToPostMessageResponse();
        return Ok(response);
        // return Created(response);
    } 
    public Task<IActionResult> GetRecentMessagesAsync()
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