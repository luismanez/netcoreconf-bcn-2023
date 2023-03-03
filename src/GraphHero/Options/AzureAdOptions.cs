using System.Text;
using Microsoft.Extensions.Options;

namespace GraphHero.Options;

public sealed class AzureAdOptions
{
    public string Authority => $"{Instance}/{TenantId}";
    public static string Instance => "https://login.microsoftonline.com";
    public string Domain { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string UserName { get; set; } = "";
    public string UserPassword { get; set; } = "";
}

public sealed class AzureAdOptionsValidation : IValidateOptions<AzureAdOptions>
{
    public ValidateOptionsResult Validate(string name, AzureAdOptions options)
    {
        var errorMessagesBuilder = new StringBuilder();

        if (!Guid.TryParse(options.TenantId, out var _))
            errorMessagesBuilder.Append("Invalid TenantId: ").Append(options.TenantId);

        if (!Guid.TryParse(options.ClientId, out var _))
            errorMessagesBuilder.Append("Invalid ClientId: ").Append(options.ClientId);

        if (errorMessagesBuilder.Length > 0)
            return ValidateOptionsResult.Fail(errorMessagesBuilder.ToString());

        return ValidateOptionsResult.Success;
    }
}