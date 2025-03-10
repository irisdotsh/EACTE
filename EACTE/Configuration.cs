using Dalamud.Configuration;
using System;

namespace EACTE;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int Seconds = 5;
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
