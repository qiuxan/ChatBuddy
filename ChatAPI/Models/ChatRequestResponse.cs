namespace ChatAPI.Models;

public class ChatRequest
{
    public string Query { get; set; } = string.Empty;
    
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}