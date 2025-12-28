namespace Filer.Utilities;

public static class EnvironmentVariables
{
    public static string FilerUrl =>
        Environment.GetEnvironmentVariable("FILER_URL") ?? throw new Exception("erreur");
    public static string JwtSecret =>
        Environment.GetEnvironmentVariable("JWT_SECRET")  ?? throw new Exception("erreur");
}
