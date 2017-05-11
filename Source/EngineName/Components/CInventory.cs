using EngineName.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components
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
                if (inventory.Count == MAXSIZE)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// The items contained in an inventory
        /// </summary>
        public List<int> inventory = new List<int>(MAXSIZE);
        /// <summary>
        /// Items that are to be removed from the inventory.
        /// </summary>
        public List<int> itemsToRemove = new List<int>(MAXSIZE);

        public void removeItems()
        {
            foreach (var item in itemsToRemove)
                inventory.Remove(item);
            itemsToRemove.Clear();
        }
    }
}
