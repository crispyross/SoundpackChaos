using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SoundpackLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SoundpackChaos;


public record MetaData
{
    public const string PLUGIN_NAME = "SoundpackChaos";
    public const string PLUGIN_GUID = "org.crispykevin.soundpackchaos";
    public const string PLUGIN_VERSION = "1.0.0";
}

[BepInPlugin(MetaData.PLUGIN_GUID, MetaData.PLUGIN_NAME, MetaData.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger = null!;
    public static ConfigEntry<bool> Enabled = null!;
    public static ConfigEntry<bool> IncludeVanilla = null!;

    public Plugin()
    {
        Logger = base.Logger;
    }

    private void Awake()
    {
        new Harmony(MetaData.PLUGIN_GUID).PatchAll();

        Enabled = Config.Bind("General", "Enabled", true);
        IncludeVanilla = Config.Bind("General", "Include Vanilla Soundpacks", true);

        var page = OptionalTrombSettings.GetConfigPage("SoundpackChaos");
        if (page != null)
        {
            OptionalTrombSettings.Add(page, Enabled);
            OptionalTrombSettings.Add(page, IncludeVanilla);
        }
        
        Logger.LogInfo($"Plugin {MetaData.PLUGIN_GUID} v{MetaData.PLUGIN_VERSION} is loaded!");
    }
}

[HarmonyPatch(typeof(GameController))]
class GameControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameController.playNote))]
    static void BeforeNote(GameController __instance)
    {
        if (Plugin.Enabled.Value)
        {
            var packs = Plugin.IncludeVanilla.Value ? SoundpackManager.GetAllPacks() : SoundpackManager.GetCustomPacks();
            SoundpackManager.ChangePack(__instance, packs.GetRandom());
        }
    }
}
