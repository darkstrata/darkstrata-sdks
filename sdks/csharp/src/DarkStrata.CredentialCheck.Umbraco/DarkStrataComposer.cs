using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace DarkStrata.CredentialCheck.Umbraco;

public sealed class DarkStrataComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<DarkStrataOptions>(builder.Config.GetSection(DarkStrataOptions.SectionName));

        builder.Services.AddSingleton<IDarkStrataCredentialCheck>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DarkStrataOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                sp.GetRequiredService<ILogger<DarkStrataComposer>>()
                    .LogWarning("DarkStrata credential checks are disabled: no API key at {Section}:ApiKey", DarkStrataOptions.SectionName);
                return new DisabledCredentialCheck();
            }

            return new DarkStrataCredentialCheck(new ClientOptions { ApiKey = options.ApiKey, BaseUrl = options.BaseUrl });
        });

        builder.Services.AddSingleton<CompromisedCredentialService>();
        builder.Services.AddScoped<IPasswordValidator<MemberIdentityUser>, BreachedPasswordValidator<MemberIdentityUser>>();
        builder.Services.AddScoped<IPasswordValidator<BackOfficeIdentityUser>, BreachedPasswordValidator<BackOfficeIdentityUser>>();
        builder.Services.AddUnique<IBackOfficeUserPasswordChecker, DarkStrataBackOfficePasswordChecker>();
    }
}
