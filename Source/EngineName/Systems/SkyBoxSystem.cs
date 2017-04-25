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

namespace EngineName.Systems
{
    public class SkyBoxSystem : EcsSystem
    {
        private GraphicsDeviceManager graphics;
        private Model skyModel;
        private BasicEffect skyEffect;
        private Matrix viewM, projM, skyworldM, world;
        private static float skyscale = 10000;
        private float slow = skyscale / 200f;  // step width of movements
        private Vector3 nullPos = Vector3.Zero;
        public override void Init()
        {
            world = Matrix.Identity;
            skyModel = Game1.Inst.Content.Load<Model>("Textures/skybox");
            skyEffect = (BasicEffect)skyModel.Meshes[0].Effects[0];
            graphics = Game1.Inst.Graphics;
        }
        public override void Update(float t, float dt)
        {
            foreach (CCamera camera in Game1.Inst.Scene.GetComponents<CCamera>().Values)
            {

                skyworldM = Matrix.CreateScale(skyscale, skyscale, skyscale) * Matrix.CreateTranslation(1081f / 2, 0, -1081f / 2);// * Matrix.Identity;
                projM = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 3, 1f, 1f, 5f * skyscale);
                viewM = camera.View;

            }
        }
        public override void Draw(float t, float dt)
        {
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            graphics.GraphicsDevice.RasterizerState = rs;
            skyEffect.World = skyworldM;
            skyEffect.View = viewM;
            skyEffect.Projection = projM;
            skyModel.Meshes[0].Draw();
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
