using Mediator;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetVersion;

public class GetVersionQueryHandler : IQueryHandler<GetVersionQuery, VersionDto>
{
    public ValueTask<VersionDto> Handle(GetVersionQuery request, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        
        var assemblyVersion = assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "Unknown";
        var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Unknown";
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";

        var result = new VersionDto
        {
            Version = version?.ToString() ?? "Unknown",
            AssemblyVersion = assemblyVersion,
            FileVersion = fileVersion,
            InformationalVersion = informationalVersion
        };

        return ValueTask.FromResult(result);
    }
}