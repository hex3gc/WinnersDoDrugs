using RoR2;
using WinnersDoDrugs.Configuration;
using System;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using WinnersDoDrugs.Items;
using RoR2.ExpansionManagement;
using System.Collections.Generic;
using System.Linq;
using RoR2.Orbs;
using RoR2.ContentManagement;

namespace WinnersDoDrugs.Equipment
{
    public static partial class EquipInit
    {
        public static TheBomb TheBomb = new TheBomb
        (
            "TheBomb",
            false,
            false,
            true
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class TheBomb : EquipBase
    {
        public override bool Enabled
        {
            get
            {
                return TheBomb_Enabled.Value;
            }
        }
        public override float Cooldown
        {
            get
            {
                return TheBomb_Cooldown.Value;
            }
        }
        public override GameObject equipPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Titan/matTitanPebble.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/gauntlets/matGTVoidTerrain.mat").WaitForCompletion();
        public override Sprite equipIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/theBomb.png");
        private GameObject _explodeEffect;
        public GameObject ExplodeEffect
        {
            get
            {
                if (_explodeEffect == null)
                {
                    _explodeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/CritGlassesVoid/CritGlassesVoidExecuteEffect.prefab").WaitForCompletion();
                }
                return _explodeEffect;
            }
            set;
        }

        public TheBomb(string _name, bool canBeRandomlyTriggered = true, bool enigmaCompatible = true, bool isLunar = false) :
        base(_name, canBeRandomlyTriggered, enigmaCompatible, isLunar)
        { }
        // Config
        public static ConfigItem<bool> TheBomb_Enabled = new ConfigItem<bool>
        (
            "Lunar equipment: The Bomb",
            "Equipment enabled",
            "Should this equipment appear in runs?",
            true
        );
        public static ConfigItem<float> TheBomb_Cooldown = new ConfigItem<float>
        (
            "Lunar equipment: The Bomb",
            "Equipment cooldown",
            "Seconds until the equipment cooldown is finished.",
            300f,
            1f,
            600f,
            1f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/theBomb.prefab");

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

        }

        // Hooks
        public override void RegisterHooks()
        {
            // Equipment action
            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipmentDef) =>
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Boolean RoR2.EquipmentSlot::PerformEquipmentAction(RoR2.EquipmentDef)' called on client");
                    return false;
                }
                if (self.equipmentDisabled)
                {
                    return false;
                }
                if (equipmentDef == EquipmentDef && self.characterBody)
                {
                    EffectData effectData = new EffectData()
                    {
                        origin = self.characterBody.corePosition,
                        scale = 99f
                    };
                    EffectManager.SpawnEffect(ExplodeEffect, effectData, true);

                    for (int i = 0; i < 10; i++)
                    {
                        Util.PlaySound("Play_artifactBoss_attack1_explode", self.characterBody.gameObject);
                        Util.PlaySound("Play_UI_podImpact", self.characterBody.gameObject);
                    }

                    BlastAttack blastAttack = new BlastAttack
                    {
                        position = self.characterBody.corePosition,
                        baseDamage = 999999999f,
                        baseForce = 100f,
                        radius = 2000f,
                        attacker = self.characterBody.gameObject,
                        inflictor = null,
                        teamIndex = TeamIndex.None,
                        crit = false,
                        procChainMask = new ProcChainMask(),
                        procCoefficient = 0f,
                        damageColorIndex = DamageColorIndex.Default,
                        falloffModel = BlastAttack.FalloffModel.None,
                        damageType = DamageType.BypassOneShotProtection,
                        attackerFiltering = AttackerFiltering.AlwaysHitSelf,
                        losType = BlastAttack.LoSType.None
                    };
                    blastAttack.Fire();

                    self.subcooldownTimer = 1f;
                    return true;
                }

                return orig(self, equipmentDef);
            };
        }

        // IDR
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            GameObject ItemDisplayPrefab = Helpers.PrepareItemDisplayModel(PrefabAPI.InstantiateClone(equipPrefab, EquipmentDef.name + "Display", false));
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            return rules;
        }
    }
}