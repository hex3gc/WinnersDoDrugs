using System.Collections.Generic;
using RoR2;

namespace WinnersDoDrugs.Items
{
    /// <summary>
    ///     Item setup
    /// </summary>
    public static partial class ItemInit
    {
        private static List<ItemBase> _itemList;
        public static List<ItemBase> ItemList
        {
            get
            {
                if (_itemList == null)
                {
                    _itemList = new List<ItemBase>();
                }
                return _itemList;
            }
            set
            {
                _itemList = value;
            }
        }

        public static void Init()
        {
            foreach(ItemBase ib in ItemList)
            {
                if (ib.RegisterItem())
                {
                    Log.Info("Added definition for item " + ib.Name);
                    ib.RegisterHooks();
                }
            }
        }

        public static void FormatDescriptions()
        {
            foreach(ItemBase ib in ItemList)
            {
                if (ib.Enabled)
                {
                    ib.FormatDescriptionTokens();
                }
            }
        }
    }
}