// Lightweight .NET 9 HTTP health check for Docker HEALTHCHECK CMD.
//
// Usage (inside container):
//   dotnet /healthcheck/HealthCheck.dll [url]
//
// Defaults to http://localhost:8080/health/live when no argument is supplied.
// Exit code 0 = healthy, exit code 1 = unhealthy or unreachable.

var url = args.Length > 0 ? args[0] : "http://localhost:8080/health/live";

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
    using var client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(5),
    };

    var response = await client.GetAsync(url, cts.Token);

    // 2xx → healthy; anything else (503, 4xx, etc.) → unhealthy
    Environment.Exit(response.IsSuccessStatusCode ? 0 : 1);
}
catch
{
    // Network unreachable, timeout, DNS failure, etc.
    Environment.Exit(1);
}
