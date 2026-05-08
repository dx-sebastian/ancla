using Ancla.Models;

namespace Ancla.Services;

public sealed class DevelopmentPortCatalog
{
    public const string AllFilterKey = "all";
    public const string DevelopmentOnlyFilterKey = "dev-only";

    private static readonly DevProfile None = new(string.Empty, string.Empty, []);
    private static readonly DevProfile Docker = new("docker", "Docker", [2375, 2376, 2377, 4789, 7946]);
    private static readonly IReadOnlyList<DevProfile> Profiles =
    [
        new("react", "React", [3000, 3001]),
        new("sails", "Sails", Enumerable.Range(1330, 11).ToArray()),
        Docker,
        new("vue", "Vue", [5173, 8080]),
        new("angular", "Angular", [4200]),
        new("ant-design", "Ant Design", [8000]),
        new("storybook", "Storybook", [6006]),
        new("aspnet", "ASP.NET", [5000, 5001])
    ];

    public IReadOnlyList<DevFilter> GetFilters()
    {
        return
        [
            new(AllFilterKey, "All ports", "Todos", null),
            new(DevelopmentOnlyFilterKey, "Software dev", "Puertos dev", null),
            .. Profiles.Select(profile => new DevFilter(profile.Key, profile.Label, profile.Label, profile.Key))
        ];
    }

    public DevProfile Match(int port, string processName, string executablePath)
    {
        if (LooksLikeDocker(processName, executablePath))
        {
            return Docker;
        }

        return Profiles.FirstOrDefault(profile => profile.Ports.Contains(port)) ?? None;
    }

    private static bool LooksLikeDocker(string processName, string executablePath)
    {
        return ContainsDockerToken(processName) || ContainsDockerToken(executablePath);
    }

    private static bool ContainsDockerToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("docker", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("com.docker", StringComparison.OrdinalIgnoreCase);
    }

    public sealed record DevProfile(string Key, string Label, IReadOnlyList<int> Ports);

    public sealed record DevFilter(string Key, string LabelEnglish, string LabelSpanish, string? ProfileKey)
    {
        public string Label => LabelEnglish;

        public string GetLabel(UiLanguage language)
        {
            return language == UiLanguage.Spanish ? LabelSpanish : LabelEnglish;
        }
    }
}
