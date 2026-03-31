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
        public static PackingPeanut PackingPeanut = new PackingPeanut
        (
            "PackingPeanut",
            [ItemTag.Utility, ItemTag.PriorityScrap],
            ItemTier.Tier1
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class PackingPeanut : ItemBase
    {
        public override bool Enabled => PackingPeanut_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Bison/matBisonMetalBall.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/packingPeanut.png");

        public PackingPeanut(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) :
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden)
        { }

        // Config
        public static ConfigItem<bool> PackingPeanut_Enabled = new ConfigItem<bool>
        (
            "Common: Packing Peanut",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> PackingPeanut_Chance = new ConfigItem<float>
        (
            "Common: Packing Peanut",
            "Chance of discovery",
            "Percent chance of finding this item whenever another item is picked up.",
            5f,
            1f,
            99f,
            0.1f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/packingPeanut.prefab");

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
            string descriptionToken = ItemDef.descriptionToken;

            LanguageAPI.AddOverlay
            (
                descriptionToken,
                String.Format
                (
                    Language.currentLanguage.GetLocalizedStringByToken(descriptionToken),
                    PackingPeanut_Chance.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            On.RoR2.Run.IsItemAvailable += (orig, self, itemIndex) =>
            {
                if (itemIndex == ItemIndex)
                {
                    return false;
                }

                return orig(self, itemIndex);
            };

            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += (orig, self, itemIndex, count) =>
            {
                orig(self, itemIndex, count);

                if (Util.CheckRoll(PackingPeanut_Chance.Value))
                {
                    self.GiveItemPermanent(ItemIndex);
                    if (self.gameObject.GetComponentInChildren<CharacterMaster>())
                    {
                        CharacterMasterNotificationQueue.PushItemNotification(self.gameObject.GetComponentInChildren<CharacterMaster>(), ItemIndex);
                    }
                }
            };

            On.RoR2.Inventory.GiveItemTemp += (orig, self, itemIndex, count) =>
            {
                orig(self, itemIndex, count);

                if (Util.CheckRoll(PackingPeanut_Chance.Value))
                {
                    self.GiveItemTemp(ItemIndex);
                    if (self.gameObject.GetComponentInChildren<CharacterMaster>())
                    {
                        CharacterMasterNotificationQueue.PushItemNotification(self.gameObject.GetComponentInChildren<CharacterMaster>(), ItemIndex, true);
                    }
                }
            };
        }
    }
}