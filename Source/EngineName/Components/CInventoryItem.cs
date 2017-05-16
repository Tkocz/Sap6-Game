using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components
{
    /// <summary>
    /// This class contains the properties for items in an inventory,
    /// so we can recreate them in the world when they are removed from inventory.
    /// Altough it only contains a CBody at the moment, having a seperate class for
    /// inventory items might be a good idea if the inventory should have the ability
    /// to hold different types of items.
    /// </summary>
    public class CInventoryItem : EcsComponent
    {
        //public float radius;
        //public Vector3 velocity = Vector3.Zero;
        //public Vector3 position;
        public CBody itemBody;

        public CInventoryItem(CBody body)
        {
            itemBody = body;
        }


    }
}
