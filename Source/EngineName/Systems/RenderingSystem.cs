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
        private Texture2D normalMap;
        public override void Init() {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
            normalMap = Game1.Inst.Content.Load<Texture2D>("Textures/water_bump");

            base.Init();
        }
        public override void Update(float t, float dt) {
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt) {
            base.Draw(t, dt);

            Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            foreach (var camera in Game1.Inst.Scene.GetComponents<CCamera>().Keys) {
                DrawScene(camera, t, dt);
            }
            // debugging for software culling
            //Console.WriteLine(string.Format("{0} meshes drawn", counter));
        }

        public void DrawScene(int cameraID, float t, float dt, int excludeEid=-1) {
            // TODO: Clean code below up, hard to read.
            CCamera camera = (CCamera)Game1.Inst.Scene.GetComponentFromEntity<CCamera>(cameraID);
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
                    if (model.model.Tag == "water") {

                        foreach (ModelMesh shit in model.model.Meshes) {

                            foreach (ModelMeshPart part in mesh.MeshParts) {                                Matrix world = mesh.ParentBone.Transform * transform.Frame;

                                part.Effect.Parameters["World"].SetValue(world);
                                part.Effect.Parameters["View"].SetValue(camera.View);
                                part.Effect.Parameters["Projection"].SetValue(camera.Projection);
                                part.Effect.Parameters["AmbientColor"].SetValue(new Vector4(0f, 0f, 1f, 1f));
                                part.Effect.Parameters["AmbientIntensity"].SetValue(0.5f);
                                
                                part.Effect.Parameters["DiffuseLightDirection"].SetValue(new Vector3(0f, -1f, 2f));
                                part.Effect.Parameters["DiffuseColor"].SetValue(new Vector4(0f, 0.8f, 0f, 1f));
                                part.Effect.Parameters["DiffuseIntensity"].SetValue(0.5f);


                                //Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform * world));
                                //part.Effect.Parameters["WorldInverseTranspose"].SetValue(world);

                                CTransform cameraTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(cameraID);

                                var viewVector = (camera.Target - cameraTransform.Position);
                                viewVector.Normalize();
                                //part.Effect.Parameters["ViewVector"].SetValue(viewVector);
                                part.Effect.Parameters["CameraPosition"].SetValue(cameraTransform.Position);


                                part.Effect.Parameters["Shininess"].SetValue(200f);
                                part.Effect.Parameters["SpecularColor"].SetValue(new Vector4(1, 1, 1, 1));
                                part.Effect.Parameters["SpecularIntensity"].SetValue(200f);

                                //effect.Parameters["ModelTexture"].SetValue(texture);
                                part.Effect.Parameters["NormalMap"].SetValue(normalMap);
                                part.Effect.Parameters["BumpConstant"].SetValue(8 + 2 * (float)Math.Cos(t));

                                part.Effect.Parameters["Time"].SetValue(t);
                                //part.Effect.Parameters["CameraPosition"].SetValue(cameraTransform.Position);
                                foreach (var pass in part.Effect.CurrentTechnique.Passes) {
                                    pass.Apply();
                                }
                            }
                            shit.Draw();
                        }
                    }
                    else if (model.material != null) {
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
