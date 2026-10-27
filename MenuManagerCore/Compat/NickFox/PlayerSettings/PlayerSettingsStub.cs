using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace PlayerSettingsStub;

// Ships with PanoramaMenuManager (PMM). PMM keeps menu settings itself, so PlayerSettings is not needed.
// CS2-SimpleAdmin still looks for plugins/PlayerSettings/PlayerSettings.dll and refuses to start without it.
public class PlayerSettingsStub : BasePlugin
{
    public override string ModuleName => "[PMM] PlayerSettings stub";
    public override string ModuleVersion => "1.3.01";
    public override string ModuleAuthor => "Rimmer";
    public override string ModuleDescription => "Empty PlayerSettings for plugins that only check the file. No settings:nfcore.";

    public override void Load(bool hotReload)
    {
        Logger.LogInformation(
            "PlayerSettings stub from PMM. It does not provide settings:nfcore. " +
            "If a plugin needs ISettingsApi, install the original PlayerSettingsCS2 + AnyBaseLibCS2 instead.");
    }
}
