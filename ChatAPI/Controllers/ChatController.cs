using System.Net.NetworkInformation;
using ChatAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using ChatResponse = ChatAPI.Models.ChatResponse;

namespace ChatAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : Controller
{
    private readonly IChatClient _chatClient;
    private readonly VectorStoreCollection<string, DocChunk> _icmCollection;

    public ChatController(
        IChatClient chatClient,
        VectorStoreCollection<string, DocChunk> icmCollection)
    {
        _chatClient = chatClient;
        _icmCollection = icmCollection;
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

    private async Task<IEnumerable<string>> SearchIcmAsync(string searchPhrase)
    {
        var nearest =  _icmCollection.SearchAsync(searchPhrase, top: 5);

        return await nearest.Select(result => result.Record.Content).ToListAsync();
    }
}