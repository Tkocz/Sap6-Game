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
            base.Draw(t, dt);

            Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            foreach (CCamera camera in Game1.Inst.Scene.GetComponents<CCamera>().Values) {
                DrawScene(camera);
            }
            // debugging for software culling
            //Console.WriteLine(string.Format("{0} meshes drawn", counter));
        }

        public void DrawScene(CCamera camera, int excludeEid=-1) {
            // TODO: Clean code below up, hard to read.

            foreach (CTransform transformComponent in Game1.Inst.Scene.GetComponents<CTransform>().Values)
            {
                transformComponent.Frame = Matrix.CreateScale(transformComponent.Scale) *
                    transformComponent.Rotation *
                    Matrix.CreateTranslation(transformComponent.Position);
            }

            int counter = 0;
            foreach (var component in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                var key = component.Key;

                if (key == excludeEid) {
                    // TODO: This is originally a hack to simplify rendering of environment maps.
                    continue;
                }

                C3DRenderable model = (C3DRenderable)component.Value;
                if (model.model == null) continue; // TODO: <- Should be an error, not silent fail?

                CTransform transform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(key);

                foreach (var mesh in model.model.Meshes)
                {

                    if (camera.Frustum.Contains(mesh.BoundingSphere.Transform(transform.Frame)) == ContainmentType.Disjoint)
                        continue;

                    // TODO: This might bug out with multiple mesh parts.
                    if (model.material != null) {
                        model.material.Model = mesh.ParentBone.Transform * transform.Frame;
                        model.material.View  = camera.View;
                        model.material.Proj  = camera.Projection;
                        model.material.Prerender();

                        var device = Game1.Inst.GraphicsDevice;

                        for (var i = 0; i < mesh.MeshParts.Count; i++) {
                            var part = mesh.MeshParts[i];
                            var effect = model.material.mEffect;

                            device.SetVertexBuffer(part.VertexBuffer);
                            device.Indices = part.IndexBuffer;

                            for (var j = 0; j < effect.CurrentTechnique.Passes.Count; j++) {
                                effect.CurrentTechnique.Passes[j].Apply();
                                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                             part.VertexOffset,
                                                             0,
                                                             part.NumVertices,
                                                             part.StartIndex,
                                                             part.PrimitiveCount);
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
                        }

                        mesh.Draw();
                    }
                }
            }
        }
    }
}
