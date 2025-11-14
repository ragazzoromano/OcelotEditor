using System.IO;
using Newtonsoft.Json;
using OcelotEditor.Models;

namespace OcelotEditor.Services;

public class JsonConfigurationService : IConfigurationService
{
    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    public OcelotConfiguration Load(string path)
    {
        var json = File.ReadAllText(path);
        var configuration = JsonConvert.DeserializeObject<OcelotConfiguration>(json, _serializerSettings);
        return configuration ?? new OcelotConfiguration();
    }

    public void Save(string path, OcelotConfiguration configuration)
    {
        var json = JsonConvert.SerializeObject(configuration, _serializerSettings);
        File.WriteAllText(path, json);
    }
}
