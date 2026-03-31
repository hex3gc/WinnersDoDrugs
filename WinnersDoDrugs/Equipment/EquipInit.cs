using System.Collections.Generic;

namespace WinnersDoDrugs.Equipment
{
    /// <summary>
    ///     Equip setup
    /// </summary>
    public static partial class EquipInit
    {
        private static List<EquipBase> _equipList;
        public static List<EquipBase> EquipList
        {
            get
            {
                if (_equipList == null)
                {
                    _equipList = new List<EquipBase>();
                }
                return _equipList;
            }
            set
            {
                _equipList = value;
            }
        }

        public static void Init()
        {
            foreach(EquipBase eb in EquipList)
            {
                if (eb.RegisterEquip())
                {
                    Log.Info("Added definition for equipment " + eb.Name);
                    eb.RegisterHooks();
                }
            }
        }

        public static void FormatDescriptions()
        {
            foreach(EquipBase eb in EquipList)
            {
                if (eb.Enabled)
                {
                    eb.FormatDescriptionTokens();
                }
            }
        }
    }
}