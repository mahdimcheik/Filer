namespace Filer.Utilities;

public static class CorsHelper
{
    public static bool IsOriginAllowed(string origin)
    {
        List<string> localUrls =
            new()
            {
                    "http://localhost",
                    "https://localhost",
                    "https://localhost:7113",
                    "http://localhost:4200",
            };
        return localUrls.Contains(origin);
    }
}
