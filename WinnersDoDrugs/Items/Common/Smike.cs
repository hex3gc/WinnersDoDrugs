using RoR2;
using System;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using UnityEngine.Networking;
using RoR2.Items;
using WinnersDoDrugs.Configuration;
using System.Collections.Generic;
using System.Linq;
using Fnaf2FoxyJumpScare;

namespace WinnersDoDrugs.Items
{
    public static partial class ItemInit
    {
        public static Smike Smike = new Smike
        (
            "Smike",
            [ItemTag.Utility],
            ItemTier.Tier1
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class Smike : ItemBase
    {
        public override bool Enabled => Smike_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/smike.png");

        public Smike(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> Smike_Enabled = new ConfigItem<bool>
        (
            "Common: Smike",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> Smike_StunChance = new ConfigItem<float>
        (
            "Common: Smike",
            "Activation chance",
            "Percent chance of activating on hit.",
            0.1f,
            1f,
            0.01f,
            0.01f
        );
        public static ConfigItem<float> Smike_StunChanceStack = new ConfigItem<float>
        (
            "Common: Smike",
            "Activation chance (per stack)",
            "Percent chance of activating on hit, per additional stack.",
            0.1f,
            1f,
            0.01f,
            0.01f
        );
        public static ConfigItem<float> Smike_StunLength = new ConfigItem<float>
        (
            "Common: Smike",
            "Stun length",
            "Length of the stun in seconds.",
            10f,
            1f,
            10f,
            1f
        );
        public static ConfigItem<float> Smike_StunRadius = new ConfigItem<float>
        (
            "Common: Smike",
            "Stun radius",
            "Radius of the stun in meters.",
            200f,
            10f,
            1000f,
            10f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/smike.prefab");
            
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
                    Smike_StunChance.Value,
                    Smike_StunChanceStack.Value,
                    Smike_StunRadius.Value,
                    Smike_StunLength.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // On-hit trigger
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victimObject) =>
            {
                orig(self, damageInfo, victimObject);

                if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && damageInfo.damage > 0f && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.master && victimObject.TryGetComponent(out CharacterBody victimBody))
                {
                    int itemCount = GetItemCountEffective(attackerBody);

                    if (itemCount > 0 && attackerBody.teamComponent && victimBody.teamComponent)
                    {
                        if (Util.CheckRoll((Smike_StunChance.Value + (Smike_StunChanceStack.Value * (itemCount - 1))) * damageInfo.procCoefficient, attackerBody.master.luck, attackerBody.master))
                        {
                            UnityEngine.Object.Instantiate(Fnaf2FoxyJumpScare.Main.Fnaf2FoxyJumpscare);

                            List<Collider> colliders = Physics.OverlapSphere(victimBody.corePosition, Smike_StunRadius.Value).ToList();
                            foreach (Collider collider in colliders)
                            {
                                GameObject gameObject = collider.gameObject;
                                if (gameObject.GetComponentInChildren<CharacterBody>())
                                {
                                    CharacterBody colliderBody = gameObject.GetComponentInChildren<CharacterBody>();
                                    if (colliderBody.healthComponent && colliderBody.teamComponent && colliderBody.teamComponent.teamIndex != attackerBody.teamComponent.teamIndex)
                                    {
                                        colliderBody.healthComponent.gameObject.TryGetComponent(out SetStateOnHurt setStateOnHurt);
                                        if (setStateOnHurt)
                                        {
                                            setStateOnHurt.SetStun(Smike_StunLength.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}