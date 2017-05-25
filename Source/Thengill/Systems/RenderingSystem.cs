using Thengill.Components.Renderable;
using Thengill.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Thengill.Components;
using Microsoft.Xna.Framework;
using Thengill.Shaders;
using Thengill.Utils;

namespace Thengill.Systems
{
    public class RenderingSystem : EcsSystem {
        private float mT;
        private float mDT;

        private IndexBuffer mBillboardIbo;
        private VertexBuffer mBillboardVbo;

        private BillboardMaterial mBillboardMat;

        public override void Init() {
            base.Init();

            var verts = new [] {
                new VertexPositionTexture { Position          = new Vector3(-0.5f, -0.5f, 0.0f),
                                            TextureCoordinate = new Vector2( 0.0f,  1.0f) },
                new VertexPositionTexture { Position          = new Vector3(-0.5f,  0.5f, 0.0f),
                                            TextureCoordinate = new Vector2( 0.0f,  0.0f) },
                new VertexPositionTexture { Position          = new Vector3( 0.5f,  0.5f, 0.0f),
                                            TextureCoordinate = new Vector2( 1.0f,  0.0f) },
                new VertexPositionTexture { Position          = new Vector3( 0.5f, -0.5f, 0.0f),
                                            TextureCoordinate = new Vector2( 1.0f,  1.0f) },
            };

            var indices = new short[6] { 0, 2, 1, 0, 3, 2 };



            var device = Game1.Inst.GraphicsDevice;

            mBillboardVbo = new VertexBuffer(device,
                                             VertexPositionTexture.VertexDeclaration,
                                             verts.Length,
                                             BufferUsage.None);
            mBillboardVbo.SetData<VertexPositionTexture>(verts);

            mBillboardIbo = new IndexBuffer(device,
                                            typeof (short),
                                            indices.Length,
                                            BufferUsage.None);
            mBillboardIbo.SetData<short>(indices);

            mBillboardMat = new BillboardMaterial();
        }
        public override void Update(float t, float dt) {
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt) {
            mT = t;
            mDT = dt;
            base.Draw(t, dt);

            foreach (var camera in Game1.Inst.Scene.GetComponents<CCamera>()) {
                DrawScene((CCamera)(camera.Value), -1);
                break;
            }
            // debugging for software culling
            //Console.WriteLine(string.Format("{0} meshes drawn", counter));
        }

        public void DrawScene(CCamera camera, int excludeEid=-1) {
            // TODO: Clean code below up, hard to read.

            Game1.Inst.GraphicsDevice.Clear(new Color(Game1.Inst.Scene.LightConfig.ClearColor));

            Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (CTransform transformComponent in Game1.Inst.Scene.GetComponents<CTransform>().Values)
            {
                transformComponent.Frame = Matrix.CreateScale(transformComponent.Scale) *
                    transformComponent.Rotation *
                    Matrix.CreateTranslation(transformComponent.Position);
            }

            var numDraws1 = 0;
            var numDraws = 0;

            var device = Game1.Inst.GraphicsDevice;
            var scene = Game1.Inst.Scene;
            var frustrum = camera.Frustum;

            foreach (var component in scene.GetComponents<C3DRenderable>()) {
                numDraws1++;
                var key = component.Key;

                if (key == excludeEid) {
                    // TODO: This is originally a hack to simplify rendering of environment maps.
                    continue;
                }

                C3DRenderable model = (C3DRenderable)component.Value;
                if (model.model == null) continue; // TODO: <- Should be an error, not silent fail?

                CTransform transform = (CTransform)scene.GetComponentFromEntity<CTransform>(key);

                Matrix[] bones = new Matrix[model.model.Bones.Count];
                model.model.CopyAbsoluteBoneTransformsTo(bones);

                var anim = Matrix.Identity;
                if (model.animFn != null) {
                    anim = model.animFn(mT);
                }

                if (Game1.Inst.Scene.EntityHasComponent<CPlayer>(key)) {
                    CPlayer playerData = (CPlayer)Game1.Inst.Scene.GetComponentFromEntity<CPlayer>(key);
                    if (playerData.IsAttacking) {
                        bones[1] *= Matrix.CreateTranslation(0.0f, 0.5f, 0f);
                        bones[1] *= Matrix.CreateRotationX(2*(float)Math.Sin(-playerData.AnimationProgress));
                        bones[1] *= Matrix.CreateTranslation(0.0f, -0.5f, 0f);
                    }
                }

                for(int k = 0; k < model.model.Meshes.Count; k++)
                {
                    var mesh = model.model.Meshes[k];
                    string tag = model.model.Tag as string;
                    // TODO: This is a really ugly hack. :-(
                    if (tag != "Heightmap")
                    {
                        if (frustrum.Contains(mesh.BoundingSphere.Transform(transform.Frame)) ==
                            ContainmentType.Disjoint)
                        {
                            break;
                        }
                    }

                    Effect lastEffect = null;
                    for (var i = 0; i < mesh.MeshParts.Count; i++) {
                        var part = mesh.MeshParts[i];

                        if (part.PrimitiveCount == 0) {
                            continue;
                        }

                        MaterialShader mat = null;

                        if (model.materials != null) {
                            model.materials.TryGetValue(i, out mat);
                        }

                        var effect = mat?.mEffect ?? part.Effect;

                        if (lastEffect != effect) {
                            if (mat != null) {
                                mat.CamPos = camera.Position;
                                mat.Model  = bones[mesh.ParentBone.Index] * anim * transform.Frame;
                                mat.View   = camera.View;
                                mat.Proj   = camera.Projection;
                                mat.FogStart = Game1.Inst.Scene.LightConfig.FogStart;
                                mat.FogEnd = Game1.Inst.Scene.LightConfig.FogEnd;
                                mat.FogColor = Game1.Inst.Scene.LightConfig.ClearColor;
                                mat.Prerender();
                            }
                            else {
                                SetupBasicEffect((BasicEffect)effect,
                                                 camera,
                                                 model,
                                                 scene,
                                                 transform,
                                                 mesh,
                                                 anim,
                                                 bones[mesh.ParentBone.Index],
                                                 Game1.Inst.Scene.LightConfig);
                            }

                            lastEffect = effect;
                        }

                        device.SetVertexBuffer(part.VertexBuffer);
                        device.Indices = part.IndexBuffer;

                        for (var j = 0; j < effect.CurrentTechnique.Passes.Count; j++) {
                            effect.CurrentTechnique.Passes[j].Apply();

                            numDraws++;
                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                         part.VertexOffset,
                                                         0,
                                                         part.NumVertices,
                                                         part.StartIndex,
                                                         part.PrimitiveCount);
                        }
                    }
                }
            }

            //--------------------
            // Billboards
            //--------------------

            device.SetVertexBuffer(mBillboardVbo);
            device.Indices = mBillboardIbo;

            var bbMat = mBillboardMat;
            var pass0 = bbMat.mEffect.CurrentTechnique.Passes[0];

            bbMat.CamPos = camera.Position;
            bbMat.Proj   = camera.Projection;
            bbMat.View   = camera.View;
            bbMat.FogStart = scene.LightConfig.FogStart;
            bbMat.FogEnd = scene.LightConfig.FogEnd;
            bbMat.FogColor = scene.LightConfig.ClearColor;

            var camPos = camera.Position;
            var o = camPos - camera.Target;
            o.Normalize();
            o *= 2.0f;

            foreach (var e in scene.GetComponents<CBillboard>()) {
                var bb    = (CBillboard)e.Value;
                var bbRot = Matrix.CreateBillboard(bb.Pos, camPos + o, Vector3.Up, null);

                bbMat.Tex   = bb.Tex;
                bbMat.Model = Matrix.CreateScale(bb.Scale) * bbRot;
                bbMat.Prerender();
                pass0.Apply();

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            foreach (var component in scene.GetComponents<CWater>()) {
                var model = (C3DRenderable)component.Value;
                var key = component.Key;

                if (key == excludeEid) {
                    // TODO: This is originally a hack to simplify rendering of environment maps.
                    continue;
                }
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
                        part.Effect.Parameters["CameraPosition"].SetValue(camera.Position);
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

        private void SetupBasicEffect(BasicEffect   effect,
                                      CCamera       camera,
                                      C3DRenderable model,
                                      Scene         scene,
                                      CTransform    transform,
                                      ModelMesh     mesh,
                                      Matrix        anim,
                                      Matrix        boneTransform,
                                      LightingConfig config)
        {
            effect.EnableDefaultLighting();

            effect.PreferPerPixelLighting = true;
            effect.VertexColorEnabled     = model.enableVertexColor;
            effect.LightingEnabled        = true;
            effect.AmbientLightColor      = scene.AmbientColor;

            effect.DirectionalLight0.Enabled       = true;
            effect.DirectionalLight0.Direction     = scene.Direction;
            effect.DirectionalLight0.DiffuseColor  = scene.DiffuseColor;
            effect.DirectionalLight0.SpecularColor = scene.SpecularColor * model.specular;

            effect.DirectionalLight1.Enabled       = true;
            effect.DirectionalLight1.DiffuseColor  = scene.DiffuseColor*0.7f;
            effect.DirectionalLight1.SpecularColor = scene.SpecularColor * model.specular;

            effect.DirectionalLight2.Enabled       = true;
            effect.DirectionalLight2.DiffuseColor  = scene.DiffuseColor*0.5f;
            effect.DirectionalLight2.SpecularColor = scene.SpecularColor * model.specular;

            effect.SpecularPower = 100;

            effect.FogEnabled = config.FogEnabled;
            effect.FogStart   = config.FogStart;
            effect.FogEnd     = config.FogEnd;
            effect.FogColor   = config.ClearColor;

            effect.Projection = camera.Projection;
            effect.View       = camera.View;
            effect.World      = boneTransform * anim * transform.Frame;
        }
    }

}
