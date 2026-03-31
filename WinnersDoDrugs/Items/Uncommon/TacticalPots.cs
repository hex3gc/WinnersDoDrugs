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
using RoR2.ExpansionManagement;

namespace WinnersDoDrugs.Items
{
    public static partial class ItemInit
    {
        public static TacticalPots TacticalPots = new TacticalPots
        (
            "TacticalPots",
            [ItemTag.Utility, ItemTag.Damage],
            ItemTier.Tier2
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class TacticalPots : ItemBase
    {
        public override bool Enabled => TacticalPots_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Titan/matTitanPebble.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/gauntlets/matGTVoidTerrain.mat").WaitForCompletion();
        public Material material2 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/gauntlets/matGTVoidTerrain.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/tacticalPots.png");
        private GameObject _potPrefab;
        public GameObject PotPrefab
        {
            get
            {
                if (_potPrefab == null)
                {
                    _potPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/SpawnPointExplosivePotBody.prefab").WaitForCompletion();
                }
                return _potPrefab;
            }
            set;
        }

        public TacticalPots(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) :
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden)
        { }

        // Config
        public static ConfigItem<bool> TacticalPots_Enabled = new ConfigItem<bool>
        (
            "Uncommon: 100 Mired Urns",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<int> TacticalPots_Pots = new ConfigItem<int>
        (
            "Uncommon: 100 Mired Urns",
            "Pot count",
            "Amount of tactical pots emitted in the interval per stack.",
            50,
            10f,
            100f,
            1f
        );
        public static ConfigItem<float> TacticalPots_Interval = new ConfigItem<float>
        (
            "Uncommon: 100 Mired Urns",
            "Pot interval",
            "How often tactical pots are deployed in seconds.",
            10f,
            1f,
            10f,
            0.5f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SiphonOnLowHealth/DisplaySiphonOnLowHealth.prefab").WaitForCompletion(), "Jesus of Nazareth");
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
                    TacticalPots_Pots.Value,
                    TacticalPots_Interval.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // Add/remove behavior on inventory change
            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);

                PotBehavior behavior = self.GetComponent<PotBehavior>();
                int itemCount = GetItemCountEffective(self);

                if (GetItemCountEffective(self) > 0 && !behavior)
                {
                    behavior = self.AddItemBehavior<PotBehavior>(itemCount);
                }

                if (GetItemCountEffective(self) <= 0 && behavior)
                {
                    UnityEngine.Object.Destroy(self.GetComponent<PotBehavior>());
                }
            };
        }

        public class PotBehavior : CharacterBody.ItemBehavior
        {
            public float convertInterval = TacticalPots_Interval.Value;
            public float convertTimer = 0f;
            public float internalInterval = 0.05f;
            public float internalTimer = 0f;
            public int storedPots = 0;

            void FixedUpdate()
            {
                convertTimer += Time.fixedDeltaTime;
                internalTimer += Time.fixedDeltaTime;

                // Convert pickup
                if (convertTimer >= convertInterval)
                {
                    storedPots += ItemInit.TacticalPots.GetItemCountEffective(body) * TacticalPots_Pots.Value;

                    Vector3 overheadPosition = new Vector3(body.corePosition.x, body.corePosition.y + 3, body.corePosition.z);
                    for (int i = 0; i < ItemInit.TacticalPots.GetItemCountEffective(body) * TacticalPots_Pots.Value; i++)
                    {
                        GameObject pot = GameObject.Instantiate(ItemInit.TacticalPots.PotPrefab, UnityEngine.Random.insideUnitSphere * 2 + overheadPosition, Quaternion.identity);
                        DestroyOnTimer killTimer = pot.AddComponent<DestroyOnTimer>();
                        killTimer.SetDuration(1f);
                        Util.PlaySound("Play_voidBarnacle_m1_chargeUp", pot);
                    }

                    convertTimer = 0f;
                }

                /*
                if (internalTimer >= internalInterval)
                {
                    if (storedPots > 0)
                    {
                        Vector3 overheadPosition = new Vector3(body.corePosition.x, body.corePosition.y + 4, body.corePosition.z);
                        GameObject pot = GameObject.Instantiate(ItemInit.TacticalPots.PotPrefab, UnityEngine.Random.insideUnitSphere * 1.5f + overheadPosition, Quaternion.identity);
                        Rigidbody potRigidBody = pot.GetComponent<Rigidbody>();
                        potRigidBody.AddForce(new Vector3(0, 1, 0), ForceMode.Impulse);
                        Util.PlaySound("Play_voidBarnacle_m1_chargeUp", pot);

                        storedPots--;
                    }

                    internalTimer = 0f;
                }
                */
            }
        }
    }
}