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
                    "http://localhost:8888",
                    "https://localhost:8888",
                    "http://filer:8888",
                    "https://filer:8888",
                    "http://filer",
                    "https://filer",


            };
        return localUrls.Contains(origin);
    }
}
