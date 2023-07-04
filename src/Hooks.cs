using Menu.Remix;
using RWCustom;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

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

        var thisModButton = self.modButtons.FirstOrDefault(x => x.ModID == Plugin.MOD_ID);
        if (thisModButton == null) return;

        var buttonPos = thisModButton.ScreenPos;
        var mousePos = self.Menu.mousePosition;

        var xDiff = Mathf.Abs(buttonPos.x - mousePos.x);
        var dist = Custom.Dist(buttonPos, mousePos);

        var inRange = dist < 150.0f;

        if (inRange && xDiff < 150.0f)
            module.MoveCounter = 3;

        var wait = Custom.LerpMap(dist, 150.0f, 50.0f, 3, 0);
        module.Timer++;

        if (module.Timer <= wait) return;
        module.Timer = 0;

        if (module.MoveCounter <= 0) return;
        module.MoveCounter--;


        var yDiff = buttonPos.y - mousePos.y;

        if (inRange)
            module.Dir = yDiff <= 0 ? 1 : -1;

        var index = thisModButton.viewIndex + module.Dir;

        if (thisModButton.selectOrder >= self._currentSelections.Length - 1)
        {
            index = 0;
            module.MoveCounter++;
        }
        else if (thisModButton.selectOrder <= 0)
        {
            index = self._currentSelections.Length - 1;
            module.MoveCounter++;
        }

        var nextModButton = self.visibleModButtons[index];
        
        (thisModButton.selectOrder, nextModButton.selectOrder) = (nextModButton.selectOrder, thisModButton.selectOrder);
        self.RefreshAllButtons();
    }
}
