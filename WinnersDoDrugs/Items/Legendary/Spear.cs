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
        public static Spear Spear = new Spear
        (
            "Spear",
            [ItemTag.Damage],
            ItemTier.Tier3
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class Spear : ItemBase
    {
        public override bool Enabled => Spear_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/DLC2/matShrineofRebirth.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/DLC2/meridian/Assets/matPMGlow.mat").WaitForCompletion();
        public Material material2 => Addressables.LoadAssetAsync<Material>("RoR2/DLC2/meridian/Assets/matPMGlow.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/spear.png");

        public Spear(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) :
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden)
        { }

        // Config
        public static ConfigItem<bool> Spear_Enabled = new ConfigItem<bool>
        (
            "Legendary: Spear of the Seven Dark Flames of the Nights Eclipse Forged by the Gods Under the Watch of Ten Thousand Honored Knights Receive This Item to Wield Untold Powers Beyond All Others",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<bool> Spear_True = new ConfigItem<bool>
        (
            "Legendary: Spear of the Seven Dark Flames of the Nights Eclipse Forged by the Gods Under the Watch of Ten Thousand Honored Knights Receive This Item to Wield Untold Powers Beyond All Others",
            "True",
            "Can I get a 'True'?",
            true
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/spear.prefab");

            Material[] materials =
            {
                material0,
                material1,
                material2
            };
            ret.GetComponentInChildren<MeshRenderer>().SetMaterialArray(materials);

            return ret;
        }


        // Tokens
        public override void FormatDescriptionTokens()
        {
            
        }

        // Hooks
        public override void RegisterHooks()
        {
            // 1 damage
            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, damageInfo) =>
            {
                CharacterBody victimBody = self.body;

                if (damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && victimBody)
                {
                    int itemCount = GetItemCountEffective(attackerBody);

                    if (itemCount > 0)
                    {
                        damageInfo.damage += 1f;
                    }
                }

                orig(self, damageInfo);
            };
        }
    }
}