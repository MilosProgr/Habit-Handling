//using System.IdentityModel.Tokens.Jwt;
namespace DevHabit.Api.DTOs.Auth;

public sealed record TokenRequest(string userId, string Email, IEnumerable<string> Roles)
{
    public TokenRequest() : this(string.Empty, string.Empty, Enumerable.Empty<string>())
    {
    }
}
