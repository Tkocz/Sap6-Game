using System;
using Thengill;
using Thengill.Components.Renderable;
using Thengill.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GameName.Scenes
{
    class EndGameScene : Scene
    {
        private const float lifeTime = 30.0f;
        private float passedTime = 0.0f;
        private float gameTime;
        private int score;
        private bool won;

        public EndGameScene(float gameTime, int score, bool won)
        {

            this.gameTime = gameTime;
            this.score = score;
            this.won = won;
        }

        public override void Init()
        {
            AddSystem(new Rendering2DSystem());
            base.Init();
            int textTopId = AddEntity();
            int textBottomId = AddEntity();
            string textTop = won ? "You win" : "You lose";            
            textTop = string.Format("Game Over, {0}", textTop);
            string textBottom = string.Format("\nNumber of animals killed: {0}", score);
            SpriteFont font = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans");
            AddComponent<C2DRenderable>(textTopId, new CText()
            {
                
                font = font,
                format = textTop,
                color = Color.Black,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f - (font.MeasureString(textTop).X * 0.5f), Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f - (font.MeasureString(textBottom).Y * 0.5f)),
                origin = Vector2.Zero

            });
            AddComponent<C2DRenderable>(textBottomId, new CText() {

                font = font,
                format = textBottom,
                color = Color.Black,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f - (font.MeasureString(textBottom).X * 0.5f), Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f + (font.MeasureString(textBottom).Y * 0.5f)),
                origin = Vector2.Zero

            });
        }

        public override void Update(float t, float dt)
        {
            passedTime += dt;
            if (passedTime > lifeTime)
            {
                Game1.Inst.LeaveScene();
                Game1.Inst.EnterScene(new MainMenu(null));
            }
            base.Update(t, dt);
        }
        public override void Draw(float t, float dt)
        {
            Game1.Inst.GraphicsDevice.Clear(Color.White);
            base.Draw(t, dt);
        }
    }
}
