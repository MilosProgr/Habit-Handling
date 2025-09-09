//using System.IdentityModel.Tokens.Jwt;
namespace DevHabit.Api.DTOs.Auth;

public sealed record AccessTokensDto(string AccessToken,string RefreshToken);
