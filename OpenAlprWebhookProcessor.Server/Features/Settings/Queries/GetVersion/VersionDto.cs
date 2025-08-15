namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetVersion;

public class VersionDto
{
    public string Version { get; set; } = string.Empty;
    public string AssemblyVersion { get; set; } = string.Empty;
    public string FileVersion { get; set; } = string.Empty;
    public string InformationalVersion { get; set; } = string.Empty;
}