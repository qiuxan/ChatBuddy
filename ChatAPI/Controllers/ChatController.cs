using System.Net.NetworkInformation;
using ChatAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : Controller
{
    [HttpPost]
    public ChatResponse Ping([FromBody] ChatRequest request)
    {
        return new ChatResponse()
        {
            Message = request.Query,
            Status = "Success"
        };
    }
}