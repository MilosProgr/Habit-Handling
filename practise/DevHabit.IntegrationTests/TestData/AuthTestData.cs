using DevHabit.Api.DTOs.Auth;

namespace DevHabit.IntegrationTests.TestData;

public static class AuthTestData
{
    private const string ValidTestEmail = "test@example.com";
    private const string ValidTestPassword = "StrongPass12345!";

    public static RegisterUserDto ValidRegisterUserDto => new()
    {
        Name = ValidTestEmail,
        Email = ValidTestEmail,
        Password = ValidTestPassword,
        ConfirmPassword = ValidTestPassword,
    };

    public static LoginUserDto ValidLoginUserDto => new()
    {
        Email = ValidTestEmail,
        Password = ValidTestPassword,
    };
}
