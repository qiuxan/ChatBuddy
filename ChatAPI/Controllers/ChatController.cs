using System.Net.NetworkInformation;
using ChatAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ChatResponse = ChatAPI.Models.ChatResponse;

namespace ChatAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : Controller
{
    private readonly IChatClient _chatClient;

    public ChatController(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }
    [HttpPost]
    public async Task<ChatResponse> Ping([FromBody] ChatRequest request)
    {
        List<ChatMessage> messages =
        [
            new ChatMessage(ChatRole.System, "Your are a general Chat System"),
            new ChatMessage(ChatRole.User, request.Query),
        ];

        var response = await _chatClient.GetResponseAsync(messages, new ChatOptions());
        
        return new ChatResponse()
        {
            Message = response.Text,
            Status = "Success"
        };
    }
}