using System;
using HarmonyLib;
using IL.RoR2.UI;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WinnersDoDrugs.Equipment
{
    /// <summary>
    ///     Abstract equip used for initializing new equipments.
    /// </summary>
    public abstract class EquipBase
    {
        public EquipmentDef EquipmentDef;
        public EquipmentIndex EquipmentIndex
        {
            get
            {
                return EquipmentCatalog.FindEquipmentIndex(EquipmentDef.name);
            }
        }
        public abstract bool Enabled
        {
            get;
        }
        public abstract float Cooldown
        {
            get;
        }
        public abstract GameObject equipPrefab
        {
            get;
        }
        public abstract Sprite equipIcon
        {
            get;
        }
        public string Name;
        public bool CanBeRandomlyTriggered;
        public bool EnigmaCompatible;
        public bool IsLunar;

        public EquipBase(string _name, bool _canBeRandomlyTriggered, bool _enigmaCompatible, bool _isLunar)
        {
            Name = _name;
            CanBeRandomlyTriggered = _canBeRandomlyTriggered;
            EnigmaCompatible = _enigmaCompatible;
            IsLunar = _isLunar;

            EquipInit.EquipList.Add(this);
        }

        public bool RegisterEquip()
        {
            if (!Enabled)
            {
                return Enabled;
            }

            EquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            EquipmentDef.name = Name;
            EquipmentDef.nameToken = "WDD_EQUIP_" + Name.ToUpper() + "_NAME";
            EquipmentDef.pickupToken = "WDD_EQUIP_"+ Name.ToUpper() + "_PICKUP";
            EquipmentDef.descriptionToken = "WDD_EQUIP_" + Name.ToUpper() + "_DESC";
            EquipmentDef.loreToken = "WDD_EQUIP_" + Name.ToUpper() + "_LORE";

            EquipmentDef.requiredExpansion = Main.Expansion;

            EquipmentDef.pickupModelPrefab = equipPrefab;
            EquipmentDef.pickupIconSprite = equipIcon;

            EquipmentDef.appearsInSinglePlayer = true;
            EquipmentDef.appearsInMultiPlayer = true;
            EquipmentDef.canDrop = true;
            EquipmentDef.cooldown = Cooldown;

            EquipmentDef.isBoss = false;
            EquipmentDef.isConsumed = false;
            EquipmentDef.isLunar = IsLunar;

            EquipmentDef.canBeRandomlyTriggered = CanBeRandomlyTriggered;
            EquipmentDef.enigmaCompatible = EnigmaCompatible;

            if (equipPrefab)
            {
                Transform child = equipPrefab.transform.GetChild(0);
                ModelPanelParameters modelPanelParameters = equipPrefab.AddComponent<ModelPanelParameters>();
                modelPanelParameters.minDistance = 0.5f;
                modelPanelParameters.maxDistance = 1f;
                modelPanelParameters.focusPointTransform = child;
                modelPanelParameters.cameraPositionTransform = child;
            }
            
            ItemAPI.Add(new CustomEquipment(EquipmentDef, CreateItemDisplayRules()));
            
            return Enabled;
        }

        public abstract void FormatDescriptionTokens();
        public abstract void RegisterHooks();
        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
    }
}