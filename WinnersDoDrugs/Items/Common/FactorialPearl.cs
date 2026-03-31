using RoR2;
using System;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using UnityEngine.Networking;
using RoR2.Items;
using WinnersDoDrugs.Configuration;

namespace WinnersDoDrugs.Items
{
    public static partial class ItemInit
    {
        public static FactorialPearl FactorialPearl = new FactorialPearl
        (
            "FactorialPearl",
            [ItemTag.Healing],
            ItemTier.Tier1
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class FactorialPearl : ItemBase
    {
        public override bool Enabled => DemonTime_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/ShinyPearl/matShinyPearl.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/factorialPearl.png");

        public FactorialPearl(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) :
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden)
        { }

        // Config
        public static ConfigItem<bool> DemonTime_Enabled = new ConfigItem<bool>
        (
            "Common: Pearl",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/factorialPearl.prefab");

            Material[] materials =
            {
                material0
            };
            ret.GetComponentInChildren<MeshRenderer>().SetMaterialArray(materials);

            return ret;
        }


        // Tokens
        public override void FormatDescriptionTokens()
        {
            /*
            string descriptionToken = ItemDef.descriptionToken;

            LanguageAPI.AddOverlay
            (
                descriptionToken,
                String.Format
                (
                    Language.currentLanguage.GetLocalizedStringByToken(descriptionToken),
                    DemonTime_TimeScale.Value * 100f
                )
            );
            */
        }

        // Hooks
        public override void RegisterHooks()
        {
            // Health boost
            RecalculateStatsAPI.GetStatCoefficients += (orig, self) =>
            {
                int itemCount = GetItemCountEffective(orig);
                if (itemCount > 0)
                {
                    int value = itemCount;
                    for (int i = itemCount - 1; i > 0; i--)
                    {
                        value = value * i;
                    }

                    self.baseHealthAdd += value;
                    
                    if (orig.modelLocator)
                    {
                        orig.modelLocator.modelTransform.localScale = new Vector3(itemCount, 1, 1);
                    }
                }
            };
        }
    }
}