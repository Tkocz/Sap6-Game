using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: SkyBoxSystem should really be moved into RenderingSystem.

namespace EngineName.Systems
{
    public class SkyBoxSystem : EcsSystem
    {
        private GraphicsDeviceManager graphics;
        private Model skyModel;
        private BasicEffect skyEffect;
        private Matrix viewM, projM, skyworldM, world;
        private static float skyscale = 800.0f;
        private float slow = skyscale / 200f;  // step width of movements
        private Vector3 nullPos = Vector3.Zero;
        public override void Init()
        {
            world = Matrix.Identity;
            skyModel = Game1.Inst.Content.Load<Model>("Models/skybox");
            skyEffect = (BasicEffect)skyModel.Meshes[0].Effects[0];
            graphics = Game1.Inst.Graphics;
        }
        public override void Update(float t, float dt)
        {
            foreach (CCamera camera in Game1.Inst.Scene.GetComponents<CCamera>().Values)
            {

                // TODO: Why are we looping through all cameras and just (re-)setting the fields
                //       below?
                projM = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 3, 1f, 1f, 5f * skyscale);
                viewM = camera.View;

            }
        }
        public override void Draw(float t, float dt)
        {
            base.Draw(t, dt);

            // TODO: CCamera is a ref-type so should really avoid realloc here even if only once
            //       every frame.
            DrawSkybox(new CCamera { View = viewM, Projection = projM });
        }

        public void DrawSkybox(CCamera cam) {
            skyworldM = Matrix.CreateScale(skyscale, skyscale, skyscale);// * Matrix.Identity;

            Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            skyEffect.World = skyworldM;
            skyEffect.View = cam.View;
            skyEffect.Projection = cam.Projection;
            skyModel.Meshes[0].Draw();
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
