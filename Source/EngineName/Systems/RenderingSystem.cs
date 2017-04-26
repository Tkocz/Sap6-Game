using EngineName.Components.Renderable;
using EngineName.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using EngineName.Components;
using Microsoft.Xna.Framework;

namespace EngineName.Systems
{
    public class RenderingSystem : EcsSystem {
        private GraphicsDevice mGraphicsDevice;

        public override void Init() {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
            base.Init();
        }
        public override void Update(float t, float dt) {
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt) {
            int counter = 0;
            foreach (CCamera camera in Game1.Inst.Scene.GetComponents<CCamera>().Values) {
                foreach (var component in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                    var key = component.Key;
                    C3DRenderable model = (C3DRenderable)component.Value;
                    if (model.model == null) continue;
                    CTransform transform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(key);    
                    
                    foreach (var mesh in model.model.Meshes)
                    {

                        if (camera.Frustum.Contains(mesh.BoundingSphere.Transform(transform.Frame)) == ContainmentType.Disjoint)
                            continue;
                        if (model.model.Tag =="water")
                        {
                            foreach (var effect in mesh.Effects)
                            {

                                /*effect.Projection = camera.Projection;
                                effect.View = camera.View;
                                effect.World = mesh.ParentBone.Transform * transform.Frame;
                                
                                */

                                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                                {
                                    pass.Apply();
                                }
                            }
                        }
                            else { 
                            foreach (BasicEffect effect in mesh.Effects) {
                                effect.EnableDefaultLighting();
                                effect.PreferPerPixelLighting = true;
                                
                                effect.Projection = camera.Projection;
                                effect.View = camera.View;
                                effect.World = mesh.ParentBone.Transform * transform.Frame;
                                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                                {
                                    pass.Apply();
                                }
                            }

                        }
                        counter++;
                        mesh.Draw();
                    }
                }
            }
            // debugging for software culling
            //Console.WriteLine(string.Format("{0} meshes drawn", counter));
        }
    }
}
