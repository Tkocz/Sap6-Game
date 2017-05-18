using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using GameName.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Systems
{
    /// <summary>
    /// This class handles updates for CInventory components.
    /// </summary>
    public class InventorySystem : EcsSystem
    {
        public override void Update(float t, float dt)
        {
            var inventories = Game1.Inst.Scene.GetComponents<CInventory>();
            foreach (var inv in inventories)
            {
                var itemsToRemove = new List<CInventoryItem>();
                var i = (CInventory)inv.Value;
                foreach (var r in i.itemsToRemove)
                {
                    var scene = (WorldScene)Game1.Inst.Scene;
                    var transform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(inv.Key);
                    CreateBall(transform.Position, transform.Rotation.Forward  * r.itemBody.Velocity);
                    itemsToRemove.Add(r);
                }
                var removeArray = itemsToRemove.ToArray();
                var idRemoveArray = i.IdsToRemove.ToArray();
                for (int x = 0; x < idRemoveArray.Length; x++)
                {
                    if (x < removeArray.Length)
                        i.itemsToRemove.Remove(removeArray[x]);
                    Game1.Inst.Scene.RemoveEntity(idRemoveArray[x]);
                }
            }
            base.Update(t, dt);
        }
        /// <summary>
        /// Create a new ball, this is the only application that this system has been used for.
        /// But CInventoryItem could contain a model which would make it possible to apply the same 
        /// </summary>
        /// <param name="p"> Initial postion of the ball. </param>
        /// <param name="v"> Initial Velocity of the ball. </param>
        /// <param name="r"> Initial rotation of the ball. </param>
        /// <returns> Returns the id of the new entity. </returns>
        private int CreateBall(Vector3 p, Vector3 v, float r = 1.0f)
        {
            var ball = Game1.Inst.Scene.AddEntity();

            Game1.Inst.Scene.AddComponent(ball, new CBody
            {
                Aabb = new BoundingBox(-r * Vector3.One, r * Vector3.One),
                Radius = r,
                LinDrag = 0.2f,
                Velocity = v,
                Restitution = 0.3f
            });
            Game1.Inst.Scene.AddComponent(ball, new CTransform
            {
                Position = p,
                Rotation = Matrix.Identity,
                Scale = r * Vector3.One
            });
            //AddComponent(ball, new CSyncObject());
            Game1.Inst.Scene.AddComponent<C3DRenderable>(ball, new CImportedModel
            {
                model = Game1.Inst.Content.Load<Model>("Models/badboll"),
                fileName = "badboll"
            });
            Game1.Inst.Scene.AddComponent(ball, new CPickUp());
            return ball;
        }
    }
}
