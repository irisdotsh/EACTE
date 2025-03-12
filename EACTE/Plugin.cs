using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using EACTE.Windows;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EACTE;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;

    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    [PluginService] internal static ICondition Condition { get; private set; } = null!;

    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/eacte";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("EACTE");

    private ConfigWindow ConfigWindow { get; init; }

    internal Stopwatch OutOfCombatTimer = new Stopwatch();


    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the configuration window."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUI;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        Log.Information("Initialized");

        Condition.ConditionChange += OnConditionChange;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        Condition.ConditionChange -= OnConditionChange;

        OutOfCombatTimer.Stop();
    }

    private void OnCommand(string command, string args)
    {
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();

    private void SendACTEndMessage()
    {
        new Task(() =>
        {
            try
            {
                Thread.Sleep(Configuration.Seconds*1000);

                ChatGui.Print(new XivChatEntry()
                {
                    Type = XivChatType.Echo,
                    Message = "end"
                });

                Log.Information($"Sent ACT end message after {Configuration.Seconds} seconds");
            }
            catch (Exception err)
            {
                Log.Warning("Unable to send ACT end message: {0}", err);
            }
        }).Start();
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.LoggingOut) return;

        if (!Condition.Any()) return;

        if (ClientState?.LocalPlayer?.ClassJob == null) return;

        if (flag == ConditionFlag.InCombat && OutOfCombatTimer.IsRunning)
        {
            Log.Debug("Exiting Combat");

            SendACTEndMessage();

            try
            {
                OutOfCombatTimer.Stop();
                OutOfCombatTimer.Reset();
            }
            catch (Exception err)
            {
                Log.Warning("Unable to stop and reset OutOfCombatTimer: {0}", err);
            }
            
        }
        else if (flag == ConditionFlag.InCombat && !OutOfCombatTimer.IsRunning)
        {
            Log.Debug("Entering Combat");

            try
            {
                OutOfCombatTimer.Start();
            }
            catch (Exception err)
            {
                Log.Warning("Unable to start OutOfCombatTimer: {0}", err);
            }
        }
    }
}
