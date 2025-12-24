namespace Filer.Utilities;

public static class EnvironmentVariables
{
    public static string FilerUrl =>
        Environment.GetEnvironmentVariable("FilerUrl") ?? " \"http://localhost:8888\"";
    public static string JwtSecret =>
        Environment.GetEnvironmentVariable("JWT_SECRET") ?? "JWT_SECRETJWT_SECRETJWT_SECRETJWT_SECRETJWT_SECRET";
}
