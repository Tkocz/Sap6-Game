using EngineName;
using EngineName.Components.Renderable;
using EngineName.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Scenes {
    class EndGameScene : Scene {
        private const float lifeTime = 10.0f;
        private float passedTime = 0.0f;
        private float gameTime;
        private int ballCount;

        public EndGameScene(float gameTime, int ballCount) {
            this.gameTime = gameTime;
            this.ballCount = ballCount;
        }
        public override void Init() {
            AddSystem(new Rendering2DSystem());

            int text = AddEntity();
            base.Init();
            AddComponent<C2DRenderable>(text, new CText() {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans"),
                format = "Din tid blev: " + gameTime.ToString() + " s" + "\n" + "Du plockade upp " + ballCount.ToString() + " bollar",
                color = Color.Black,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f, Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f),
                origin = Vector2.Zero

            });
        }

        public override void Update(float t, float dt) {

            passedTime += dt;

            if(passedTime > lifeTime) {
                Game1.Inst.LeaveScene();
                Game1.Inst.EnterScene(new MainMenu());
            }

            base.Update(t, dt);
        }

        public override void Draw(float t, float dt) {
            Game1.Inst.GraphicsDevice.Clear(Color.White);
            base.Draw(t, dt);
        }
    }
}
