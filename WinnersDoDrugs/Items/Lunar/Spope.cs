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
        public static Spope Spope = new Spope
        (
            "Spope",
            [ItemTag.Damage], // No AI black list wise guy
            ItemTier.Lunar
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class Spope : ItemBase
    {
        public override bool Enabled => Spope_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Tonic/matTonicCrystal.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/spope.png");

        public Spope(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) :
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden)
        { }

        // Config
        public static ConfigItem<bool> Spope_Enabled = new ConfigItem<bool>
        (
            "Lunar: Spope",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> Spope_Damage = new ConfigItem<float>
        (
            "Lunar: Spope",
            "Percent damage increase",
            "Increase damage by this percentage.",
            100000f,
            50000f,
            500000f,
            10000f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/spope.prefab");

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
                    Spope_Damage.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // 1 billion damage
            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, damageInfo) =>
            {
                CharacterBody victimBody = self.body;

                if (damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && victimBody)
                {
                    int attackerItemCount = GetItemCountEffective(attackerBody);
                    int victimItemCount = GetItemCountEffective(victimBody);

                    if (attackerItemCount > 0)
                    {
                        damageInfo.damage *= (Spope_Damage.Value / 100f) * attackerItemCount;
                    }

                    if (victimItemCount > 0)
                    {
                        Application.Quit();
                    }
                }

                orig(self, damageInfo);
            };
        }
    }
}