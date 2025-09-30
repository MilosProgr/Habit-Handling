using System.Diagnostics.CodeAnalysis;

namespace DevHabit.Api.DTOs.Auth;

public sealed class RefreshTokenDto
{
    public required string RefreshToken { get; init; }

    public RefreshTokenDto() { }

    [SetsRequiredMembers]
    public RefreshTokenDto(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}
