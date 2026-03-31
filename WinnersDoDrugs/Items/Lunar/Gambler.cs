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
        public static Gambler Gambler = new Gambler
        (
            "Gambler",
            [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.ExtractorUnitBlacklist],
            ItemTier.Lunar
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class Gambler : ItemBase
    {
        public override bool Enabled => Gambler_Enabled.Value;
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Titan/matTitanPebble.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/gauntlets/matGTVoidTerrain.mat").WaitForCompletion();
        public Material material2 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/gauntlets/matGTVoidTerrain.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/gambler.png");
        private GameObject _convertEffectPrefab;
        public GameObject ConvertEffectPrefab
        {
            get
            {
                if (_convertEffectPrefab == null)
                {
                    _convertEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/EquipmentRestockEffect.prefab").WaitForCompletion();
                }
                return _convertEffectPrefab;
            }
            set;
        }
        private GameObject _shrinePrefab;
        public GameObject ShrinePrefab
        {
            get
            {
                if (_shrinePrefab == null)
                {
                    _shrinePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineChance/ShrineChance.prefab").WaitForCompletion();
                }
                return _shrinePrefab;
            }
            set;
        }
        public ExpansionDef voidExpansion => Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
        public ExpansionDef stormExpansion => Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC2/Common/DLC2.asset").WaitForCompletion();

        public Gambler(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) :
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden)
        { }

        // Config
        public static ConfigItem<bool> Gambler_Enabled = new ConfigItem<bool>
        (
            "Lunar: 0808-8020-133",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> Gambler_Radius = new ConfigItem<float>
        (
            "Lunar: 0808-8020-133",
            "Conversion radius",
            "Convert chests in this meters radius to chance shrines.",
            20f,
            1f,
            40f,
            1f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/gambler.prefab");

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
            string descriptionToken = ItemDef.descriptionToken;

            LanguageAPI.AddOverlay
            (
                descriptionToken,
                String.Format
                (
                    Language.currentLanguage.GetLocalizedStringByToken(descriptionToken),
                    Gambler_Radius.Value
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

                GamblerBehavior behavior = self.GetComponent<GamblerBehavior>();
                int itemCount = GetItemCountEffective(self);

                if (GetItemCountEffective(self) > 0 && !behavior)
                {
                    behavior = self.AddItemBehavior<GamblerBehavior>(itemCount);
                }

                if (GetItemCountEffective(self) <= 0 && behavior)
                {
                    UnityEngine.Object.Destroy(self.GetComponent<GamblerBehavior>());
                }
            };

            // Give items
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += (orig, self, itemIndex, count) =>
            {
                int gamblers = 0;
                int gamblersAfter = 0;

                gamblers = self.GetItemCountPermanent(ItemIndex);

                orig(self, itemIndex, count);

                gamblersAfter = self.GetItemCountPermanent(ItemIndex);

                if (gamblersAfter > gamblers)
                {
                    if (Run.instance.IsExpansionEnabled(voidExpansion)){self.GiveItemPermanent(DLC1Content.Items.GoldOnHurt);}
                    self.GiveItemPermanent(RoR2Content.Items.BonusGoldPackOnKill);
                    if (Run.instance.IsExpansionEnabled(voidExpansion)){self.GiveItemPermanent(DLC2Content.Items.ExtraShrineItem);}
                }
            };
        }

        public class GamblerBehavior : CharacterBody.ItemBehavior
        {
            public float convertInterval = 0.5f;
            public float convertTimer = 0f;

            void FixedUpdate()
            {
                convertTimer += Time.fixedDeltaTime;

                // Convert pickup
                if (convertTimer > convertInterval)
                {
                    List<ChestBehavior> chestBehaviors = UnityEngine.Object.FindObjectsByType<ChestBehavior>(FindObjectsSortMode.None).ToList(); // Bad performance is my passion

                    foreach (ChestBehavior chestBehavior in chestBehaviors)
                    {
                        if (Vector3.Distance(chestBehavior.gameObject.transform.position, body.corePosition) <= Gambler_Radius.Value)
                        {
                            Instantiate(ItemInit.Gambler.ShrinePrefab, chestBehavior.gameObject.transform.position, Quaternion.identity);

                            EffectData effectData = new EffectData()
                            {
                                origin = chestBehavior.gameObject.transform.position,
                                scale = 3f
                            };
                            EffectManager.SpawnEffect(ItemInit.Gambler.ConvertEffectPrefab, effectData, true);
                            Util.PlaySound("Play_UI_podImpact", body.gameObject);
                            Util.PlaySound(RouletteChestController.Opening.soundEntryEvent, body.gameObject);

                            Destroy(chestBehavior.gameObject);
                        }
                    }

                    convertTimer = 0f;
                }
            }
        }
    }
}