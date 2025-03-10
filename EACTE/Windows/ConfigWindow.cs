using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EACTE.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base("EACTE###Settings_Tab")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(200, 60);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var seconds = Configuration.Seconds;

        if (ImGui.SliderInt("seconds", ref seconds, 1, 600))
        {
            Configuration.Seconds = seconds;

            Configuration.Save();
        }

        ImGui.SetTooltip("Amount of seconds to wait after exiting combat to send the end message.");
    }
}
