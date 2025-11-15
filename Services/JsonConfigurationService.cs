using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using OcelotEditor.Models;

namespace OcelotEditor.Services;

public class JsonConfigurationService : IConfigurationService
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions NormalizationSerializerOptions = new()
    {
        WriteIndented = false
    };

    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    public OcelotConfiguration Load(string path)
    {
        var json = File.ReadAllText(path);
        var normalizedJson = NormalizeJson(json);
        var configuration = JsonConvert.DeserializeObject<OcelotConfiguration>(normalizedJson, _serializerSettings);
        return configuration ?? new OcelotConfiguration();
    }

    public void Save(string path, OcelotConfiguration configuration)
    {
        var json = JsonConvert.SerializeObject(configuration, _serializerSettings);
        File.WriteAllText(path, json);
    }

    private static string NormalizeJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json, DocumentOptions);
            return JsonSerializer.Serialize(document.RootElement, NormalizationSerializerOptions);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
