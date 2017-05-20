using Thengill.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Components
{
    /// <summary>
    /// This component is used to represent an inventory.
    /// </summary>
    public class CInventory : EcsComponent
    {
        private const int MAXSIZE = 5;
        /// <summary>
        /// Indicates wheter or not the specified inventory is full.
        /// </summary>
        public bool isFull
        {
            get
            {
                if (items.Count == MAXSIZE)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// The items contained in an inventory
        /// </summary>
        public List<CInventoryItem> items = new List<CInventoryItem>(MAXSIZE);
        /// <summary>
        /// Items that are to be removed from the inventory.
        /// </summary>
        public List<CInventoryItem> itemsToRemove = new List<CInventoryItem>(MAXSIZE);
        /// <summary>
        /// Id's of entities that are now in an inventory and should therefore be removed in the coming update call.
        /// </summary>
        public List<int> IdsToRemove = new List<int>();
    }
}
