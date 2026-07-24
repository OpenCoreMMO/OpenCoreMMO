namespace NeoServer.E2E.Tests.Harness;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SkipOnGitHubActionsFactAttribute : FactAttribute
{
    public SkipOnGitHubActionsFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
        {
            Skip = "Test skipped on GitHub Actions";
        }
    }
}
