namespace Filer.Models;

public class ResponseAuth
{
    public required string Token { get; set; }
    public required int ExpiresIn { get; set; }
}
