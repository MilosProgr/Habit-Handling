namespace DevHabit.Api.DTOs.GitHub;

public sealed class StoreGitHubAccessTokenDto
{
    public required string AccessToken { get; set; } 
    public required int ExpiresInDays { get; set; }
}
