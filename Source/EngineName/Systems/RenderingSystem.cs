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
        private float mT;

        public override void Init() {
            base.Init();
        }
        public override void Update(float t, float dt) {
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt) {
            mT = t;

            base.Draw(t, dt);

            foreach (var camera in Game1.Inst.Scene.GetComponents<CCamera>()) {
                var camPos = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(camera.Key);
                DrawScene((CCamera)(camera.Value), -1, camPos);
                break;
            }
            // debugging for software culling
            //Console.WriteLine(string.Format("{0} meshes drawn", counter));
        }

        public void DrawScene(CCamera camera, int excludeEid=-1, CTransform camPos=null) {
            // TODO: Clean code below up, hard to read.

            Game1.Inst.GraphicsDevice.Clear(new Color(0.4f, 0.6f, 0.8f));

            Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (CTransform transformComponent in Game1.Inst.Scene.GetComponents<CTransform>().Values)
            {
                transformComponent.Frame = Matrix.CreateScale(transformComponent.Scale) *
                    transformComponent.Rotation *
                    Matrix.CreateTranslation(transformComponent.Position);
            }

            var scene = Game1.Inst.Scene;
            foreach (var component in scene.GetComponents<C3DRenderable>()) {
                var key = component.Key;

                if (key == excludeEid) {
                    // TODO: This is originally a hack to simplify rendering of environment maps.
                    continue;
                }


                C3DRenderable model = (C3DRenderable)component.Value;
                if (model.model == null) continue; // TODO: <- Should be an error, not silent fail?
                CTransform transform = (CTransform)scene.GetComponentFromEntity<CTransform>(key);
                var anim = Matrix.Identity;

                if (model.animFn != null) {
                    anim = model.animFn(mT);
                }

                Matrix[] bones = new Matrix[model.model.Bones.Count];
                model.model.CopyAbsoluteBoneTransformsTo(bones);
                foreach (var mesh in model.model.Meshes)
                {

                    if (camera.Frustum.Contains(mesh.BoundingSphere.Transform(transform.Frame))
                        == ContainmentType.Disjoint)
                    {
                        // TODO: This is a really ugly hack. :-(
                        if ((string)model.model.Tag != "Heightmap") {
                            break;
                        }
                    }


                    // TODO: This might bug out with multiple mesh parts.
                    if ((string)model.model.Tag == "water") {
                        // Drawn after.
                        break;
                    }
                    else if (model.material != null) {
                        model.material.CamPos = camPos.Position;
                        model.material.Model = mesh.ParentBone.Transform * anim * transform.Frame;
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
                        var lastEffect = (Effect)null;
                        for(int i = 0; i < mesh.MeshParts.Count; i++) {
                            var meshPart = mesh.MeshParts[i];
                            var effect = (BasicEffect)meshPart.Effect;

                            if (lastEffect != effect) {
                                effect.PreferPerPixelLighting = true;
                                effect.EnableDefaultLighting();
                                effect.VertexColorEnabled = model.enableVertexColor;
                                effect.LightingEnabled = true;
                                effect.AmbientLightColor = scene.AmbientColor;

                                effect.DirectionalLight0.Direction = scene.Direction;
                                effect.DirectionalLight0.DiffuseColor = scene.DiffuseColor;
                                effect.DirectionalLight0.Enabled = true;
                                effect.DirectionalLight0.SpecularColor = scene.SpecularColor * model.specular;
                                effect.DirectionalLight1.SpecularColor = scene.SpecularColor * model.specular;
                                effect.DirectionalLight1.DiffuseColor = scene.DiffuseColor*0.7f;
                                effect.DirectionalLight2.SpecularColor = scene.SpecularColor * model.specular;
                                effect.DirectionalLight2.DiffuseColor = scene.DiffuseColor*0.5f;

                                effect.SpecularPower = 100;

                                effect.FogEnabled = true;
                                effect.FogStart = 35.0f;
                                effect.FogEnd = 100.0f;
                                effect.FogColor = new Vector3(0.4f, 0.6f, 0.8f);

                                effect.Projection = camera.Projection;
                                effect.View = camera.View;
                                effect.World = mesh.ParentBone.Transform * anim * transform.Frame;
                            }

                            lastEffect = effect;
                        }

                        mesh.Draw();
                    }
                }
            }

            foreach (var component in scene.GetComponents<CWater>()) {
                var key = component.Key;

                if (key == excludeEid) {
                    // TODO: This is originally a hack to simplify rendering of environment maps.
                    continue;
                }


                C3DRenderable model = (C3DRenderable)scene.GetComponentFromEntity<C3DRenderable>(key);
                if (model.model == null) continue; // TODO: <- Should be an error, not silent fail?
                CTransform transform = (CTransform)scene.GetComponentFromEntity<CTransform>(key);

                Matrix[] bones = new Matrix[model.model.Bones.Count];
                model.model.CopyAbsoluteBoneTransformsTo(bones);
                foreach (var mesh in model.model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts) {
                        Matrix world = mesh.ParentBone.Transform * transform.Frame;

                        part.Effect.Parameters["World"].SetValue(world);
                        part.Effect.Parameters["View"].SetValue(camera.View);
                        part.Effect.Parameters["Projection"].SetValue(camera.Projection);
                        part.Effect.Parameters["CameraPosition"].SetValue(camPos.Position);


                        //part.Effect.Parameters["Shininess"].SetValue(200f);
                        //part.Effect.Parameters["SpecularColor"].SetValue(new Vector4(1, 1, 1, 1));
                        //part.Effect.Parameters["SpecularIntensity"].SetValue(200f);

                        //part.Effect.Parameters["NormalMap"].SetValue(normalMap);
                        //part.Effect.Parameters["BumpConstant"].SetValue(8 + 2 * (float)Math.Cos(mT));

                        part.Effect.Parameters["Time"].SetValue(mT);

                        foreach (var pass in part.Effect.CurrentTechnique.Passes) {
                            pass.Apply();
                        }

                        mesh.Draw();
                    }
                }


            }

            // -
        }
    }

}
