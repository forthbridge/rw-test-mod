using Menu.Remix;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ForthMod;

public static partial class Hooks
{
    public static void ApplyOnModsInit() => On.RainWorld.OnModsInit += RainWorld_OnModsInit;

    public static bool isInit = false;

    public static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            if (isInit) return;
            isInit = true;

            ApplyHooks();

            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            Plugin.MOD_NAME = mod.name;
            Plugin.VERSION = mod.version;
            Plugin.AUTHORS = mod.authors;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, ModOptions.Instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("OnModsInit:\n" + e.Message);
        }
        finally
        {
            orig(self);
        }
    }

    public static void ApplyHooks()
    {
        On.Menu.Remix.MenuModList.Update += MenuModList_Update;
    }

    public static readonly ConditionalWeakTable<MenuModList, MenuModListModule> MenuModListData = new();

    public static MenuModListModule GetMenuModListModule(this MenuModList self) => MenuModListData.GetOrCreateValue(self);

    private static void MenuModList_Update(On.Menu.Remix.MenuModList.orig_Update orig, MenuModList self)
    {
        orig(self);

        var module = self.GetMenuModListModule();
        module.Timer++;

        if (module.Timer % 10 != 0) return;


        var thisModButton = self.modButtons.FirstOrDefault(x => x.ModID == Plugin.MOD_ID);

        if (thisModButton == null) return;

        var index = self.modButtons.IndexOf(thisModButton);

        if (thisModButton.selectOrder >= self._currentSelections.Length - 1 || thisModButton.selectOrder <= 0)
            module.IsDirUp = !module.IsDirUp;        

        var dir = module.IsDirUp ? 1 : -1;

        var nextModButton = self.visibleModButtons[thisModButton.viewIndex + dir];
        
        (thisModButton.selectOrder, nextModButton.selectOrder) = (nextModButton.selectOrder, thisModButton.selectOrder);
        self.RefreshAllButtons();
    }
}
