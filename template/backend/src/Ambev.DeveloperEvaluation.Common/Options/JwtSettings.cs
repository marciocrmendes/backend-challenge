namespace Ambev.DeveloperEvaluation.Common.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
}
