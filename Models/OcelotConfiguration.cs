using System.Collections.Generic;
using Newtonsoft.Json;

namespace OcelotEditor.Models;

public class OcelotConfiguration
{
    public List<Route> Routes { get; set; } = new();

    public GlobalConfiguration GlobalConfiguration { get; set; } = new();
}

public class Route
{
    public string UpstreamPathTemplate { get; set; } = string.Empty;

    public List<string> UpstreamHttpMethod { get; set; } = new();

    public string DownstreamPathTemplate { get; set; } = string.Empty;

    public string DownstreamScheme { get; set; } = "http";

    public List<HostAndPort> DownstreamHostAndPorts { get; set; } = new();

    public bool RouteIsCaseSensitive { get; set; }

    public AuthenticationOptions AuthenticationOptions { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; set; } = new();
}

public class HostAndPort
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }
}

public class AuthenticationOptions
{
    public string AuthenticationProviderKey { get; set; } = string.Empty;

    public List<string> AllowedScopes { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; set; } = new();
}

public class GlobalConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; set; } = new();
}
