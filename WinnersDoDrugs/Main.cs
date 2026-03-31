using BepInEx;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using R2API.Utils;
using RiskOfOptions;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using R2API;
using HG.Reflection;
using RiskOfOptions.Options;
using BepInEx.Configuration;
using WinnersDoDrugs.Items;
using WinnersDoDrugs.Equipment;
// using ShaderSwapper;

namespace WinnersDoDrugs
{
    [BepInPlugin(WINNERSDODRUGS_GUID, WINNERSDODRUGS_NAME, WINNERSDODRUGS_VER)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.items", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.recalculatestats", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.brynzananas.fnaf2foxyjumpscare", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string WINNERSDODRUGS_GUID = "com.Hex3.WinnersDoDrugs";
        public const string WINNERSDODRUGS_NAME = "WinnersDoDrugs";
        public const string WINNERSDODRUGS_VER = "1.0.0";
        public static Main Instance;
        public static ExpansionDef Expansion;
        public static AssetBundle Assets;
        public static AssetBundle Assets2;
        public static ConfigEntry<bool> Config_Enabled;

        public void Awake()
        {
            Log.Init(Logger);
            Log.Info($"Init {WINNERSDODRUGS_NAME} {WINNERSDODRUGS_VER}");

            Instance = this;

            Log.Info("Creating assets...");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinnersDoDrugs.drugsvfx"))
            {
                Assets = AssetBundle.LoadFromStream(stream);
            }
            // base.StartCoroutine(Assets.UpgradeStubbedShadersAsync());

            Log.Info($"Creating config...");
            Config_Enabled = Instance.Config.Bind(new ConfigDefinition("CONFIG - IMPORTANT", "Enable custom config"), false, new ConfigDescription("Set to 'true' to enable custom configuration for this mod. False by default to allow balance changes to take effect.", null, Array.Empty<object>()));

            ModSettingsManager.SetModDescription("Adds several game-breaking and unbalanced items.");
            ModSettingsManager.SetModIcon(Assets.LoadAsset<Sprite>("Assets/icons/expansionWdd.png"));
            ModSettingsManager.AddOption
            (
                new CheckBoxOption
                (
                    Config_Enabled,
                    true
                )
            );

            Log.Info($"Creating expansion...");
            Expansion = ScriptableObject.CreateInstance<ExpansionDef>();
            Expansion.name = WINNERSDODRUGS_NAME;
            Expansion.nameToken = "WDD_EXPANSION_NAME";
            Expansion.descriptionToken = "WDD_EXPANSION_DESC";
            Expansion.iconSprite = Assets.LoadAsset<Sprite>("Assets/icons/expansionWdd.png");
            Expansion.disabledIconSprite = Assets.LoadAsset<Sprite>("Assets/icons/expansionWdd-inactive.png");
            Expansion.requiredEntitlement = null;
            ContentAddition.AddExpansionDef(Expansion);

            Log.Info($"Creating equipments...");
            EquipInit.Init();

            Log.Info($"Creating items...");
            ItemInit.Init();

            On.RoR2.RoR2Application.OnMainMenuControllerInitialized += (orig, self) =>
            {
                ItemInit.FormatDescriptions();
                orig(self);
            };

            Log.Info($"Killing Foxy...");
            Fnaf2FoxyJumpScare.Main.chance.Value = 0f;
            Fnaf2FoxyJumpScare.Main.interval.Value = 999999f;

            Log.Info($"Done");
        }
    }
}
