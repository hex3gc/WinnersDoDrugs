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
        public static DemonTime DemonTime = new DemonTime
        (
            "DemonTime",
            [ItemTag.Utility],
            ItemTier.Tier1
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class DemonTime : ItemBase
    {
        public override bool Enabled => DemonTime_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/crystalworld/matTimeCrystalSolid.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matDebugBlack.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/demonTime.png");

        public DemonTime(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> DemonTime_Enabled = new ConfigItem<bool>
        (
            "Common: Demon Time",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> DemonTime_TimeScale = new ConfigItem<float>
        (
            "Common: Demon Time",
            "Percent timescale increase",
            "How much faster (or slower) game time should be per stack of this item.",
            0.2f,
            -1f,
            1f,
            0.1f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/demonTime.prefab");

            Material[] materials =
            {
                material0,
                material1
            };
            ret.GetComponentInChildren<MeshRenderer>().SetMaterialArray(materials);

            return ret;
        }

        
        // Tokens
        public override void FormatDescriptionTokens()
        {
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
        }

        // Hooks
        public override void RegisterHooks()
        {
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += (orig, self, itemIndex, count) =>
            {
                orig(self, itemIndex, count);

                int itemCount = 0;
                itemCount += GetDemonsInTeam(TeamIndex.Player);
                itemCount += GetDemonsInTeam(TeamIndex.Monster);

                Time.timeScale = 1f + (DemonTime_TimeScale.Value * itemCount);
            };
        }


        public int GetDemonsInTeam(TeamIndex teamIndex)
        {
            int ret = 0;

            foreach (TeamComponent teamComponent in TeamComponent.GetTeamMembers(teamIndex))
            {
                if (teamComponent.body)
                {
                    ret += GetItemCountEffective(teamComponent.body);
                }
            }

            return ret;
        }
    }
}