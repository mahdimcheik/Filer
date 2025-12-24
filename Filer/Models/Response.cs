using System.Diagnostics.CodeAnalysis;

namespace Filer.Models;

public class ResponseAuth
{
    public required string Token { get; set; }
    public required int ExpiresIn { get; set; }
}

public class FileUrl
{
    public required string Url { get; set; }
    public required string Name { get; set; }
    public required long Size { get; set; }

    [SetsRequiredMembers]
    public FileUrl() { }
}
