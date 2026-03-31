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
        public static MoonWeed MoonWeed = new MoonWeed
        (
            "MoonWeed",
            [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.ExtractorUnitBlacklist],
            ItemTier.Lunar
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class MoonWeed : ItemBase
    {
        public override bool Enabled => MoonWeed_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Vagrant/matVagrantCannonGreen.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/moonWeed.png");

        public MoonWeed(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) :
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden)
        { }

        // Config
        public static ConfigItem<bool> MoonWeed_Enabled = new ConfigItem<bool>
        (
            "Lunar: Moon Weed (Weed from the Moon)",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> MoonWeed_ProcCoefficient = new ConfigItem<float>
        (
            "Lunar: Moon Weed (Weed from the Moon)",
            "Proc coefficient multiplier",
            "Multiply proc chance by this much.",
            3f,
            1.5f,
            5f,
            0.1f
        );
        public static ConfigItem<float> MoonWeed_ProcCoefficientStack = new ConfigItem<float>
        (
            "Lunar: Moon Weed (Weed from the Moon)",
            "Proc coefficient multiplier (per stack)",
            "Multiply proc chance by this much more per additional stack.",
            1f,
            0.1f,
            3f,
            0.1f
        );
        public static ConfigItem<float> MoonWeed_Rotation = new ConfigItem<float>
        (
            "Lunar: Moon Weed (Weed from the Moon)",
            "Camera rotation on hit",
            "Rotate the camera by this many degrees on hit, per stack.",
            1f,
            0.1f,
            5f,
            0.1f
        );
        public static ConfigItem<float> MoonWeed_RotationCrit = new ConfigItem<float>
        (
            "Lunar: Moon Weed (Weed from the Moon)",
            "Critical camera rotation on hit",
            "Rotate the camera in the opposite direction by this many degrees on critical hit, per stack.",
            2f,
            0.1f,
            5f,
            0.1f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/moonWeed.prefab");

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
                    MoonWeed_ProcCoefficient.Value * 100f,
                    MoonWeed_ProcCoefficientStack.Value * 100f,
                    MoonWeed_Rotation.Value,
                    MoonWeed_RotationCrit.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, damageInfo) =>
            {
                CharacterBody victimBody = self.body;
                CharacterBody attackerBody = damageInfo.attacker?.GetComponent<CharacterBody>();

                int attackerItemCount = GetItemCountEffective(attackerBody);

                if (attackerBody && attackerItemCount > 0)
                {
                    damageInfo.procCoefficient = damageInfo.procCoefficient * (MoonWeed_ProcCoefficient.Value + (MoonWeed_ProcCoefficientStack.Value * (attackerItemCount - 1)));
                }
                
                orig(self, damageInfo);

                if (!damageInfo.rejected && damageInfo.damage > 0 && damageInfo.procCoefficient > 0 && attackerBody && attackerItemCount > 0 && attackerBody.teamComponent && victimBody && victimBody.teamComponent)
                {
                    LocalUser localUser = LocalUserManager.GetFirstLocalUser();

                    if (localUser != null && localUser.cachedBody == attackerBody && localUser.cameraRigController && localUser.cameraRigController.sceneCam && attackerBody.teamComponent.teamIndex != victimBody.teamComponent.teamIndex)
                    {
                        float rotationDegrees = MoonWeed_Rotation.Value * attackerItemCount;

                        if (damageInfo.crit)
                        {
                            rotationDegrees = -MoonWeed_RotationCrit.Value * attackerItemCount;
                        }

                        Transform transform = localUser.cameraRigController.sceneCam.transform;
                        transform.Rotate(new Vector3(0, 0, rotationDegrees));
                    }
                }
            };
        }
    }
}