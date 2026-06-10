namespace ChatAPI.Models;

public class DocChunk
{
    public required Guid Key { get ; set; }
    public required string Content { get ; set; }

    public string? Context { get ; set; }
    public required string DocumentId { get ; set; }
}