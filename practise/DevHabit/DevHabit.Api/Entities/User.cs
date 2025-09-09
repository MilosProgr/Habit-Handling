namespace DevHabit.Api.Entities;

public sealed class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public static string CreateNewId() => $"h_{Guid.CreateVersion7()}";

    // koristimo da skladistimo Identity Id iz identity providera,to moze biti bilo koji identity provider,Keycloak,Azure AD,Okta,AuthB,Keycloak,etc
    public string IdentifyId { get; set; }
}
