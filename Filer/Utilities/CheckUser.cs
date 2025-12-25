using System.Security.Claims;

namespace Filer.Utilities;

public static class CheckUser
{
    public static List<Claim> GetInfosFromClaims(ClaimsPrincipal principal)
    {
        return principal.Claims.ToList();
    }
}
