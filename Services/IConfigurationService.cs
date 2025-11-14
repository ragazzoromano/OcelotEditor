using OcelotEditor.Models;

namespace OcelotEditor.Services;

public interface IConfigurationService
{
    OcelotConfiguration Load(string path);

    void Save(string path, OcelotConfiguration configuration);
}
