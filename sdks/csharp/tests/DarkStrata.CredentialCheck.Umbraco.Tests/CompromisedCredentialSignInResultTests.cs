using DarkStrata.CredentialCheck.Umbraco;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace DarkStrata.CredentialCheck.Umbraco.Tests;

public class CompromisedCredentialSignInResultTests
{
    // Login code that only knows about SignInResult must treat this exactly like SignInResult.Failed,
    // or a denied login would look like a lockout or a two-factor prompt.
    [Fact]
    public void Behaves_as_a_plain_failed_result()
    {
        SignInResult result = CompromisedCredentialSignInResult.Instance;

        Assert.False(result.Succeeded);
        Assert.False(result.IsLockedOut);
        Assert.False(result.IsNotAllowed);
        Assert.False(result.RequiresTwoFactor);
    }

    [Fact]
    public void Is_detectable_by_type()
    {
        SignInResult result = CompromisedCredentialSignInResult.Instance;

        Assert.IsType<CompromisedCredentialSignInResult>(result);
        Assert.IsNotType<CompromisedCredentialSignInResult>(SignInResult.Failed);
    }
}
