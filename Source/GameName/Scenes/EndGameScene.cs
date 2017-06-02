using System;
using Thengill;
using Thengill.Components.Renderable;
using Thengill.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
            int textGameOverId = AddEntity();
            int textNumKilledId = AddEntity();
            string textGameOver = won ? "You win" : "You lose";
            textGameOver = string.Format("Game Over, {0}", textGameOver);
            string textNumKilled = string.Format("\nNumber of animals killed: {0}", score);
            SpriteFont font = Game1.Inst.Content.Load<SpriteFont>("Fonts/FFFForward");
            AddComponent<C2DRenderable>(textGameOverId, new CText()
            {
                
                font = font,
                format = textGameOver,
                color = Color.Black,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f - (font.MeasureString(textGameOver).X * 0.5f), Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f - (font.MeasureString(textGameOver).Y * 0.5f)),
                origin = Vector2.Zero

            });
            AddComponent<C2DRenderable>(textNumKilledId, new CText() {

                font = font,
                format = textNumKilled,
                color = Color.Black,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f - (font.MeasureString(textNumKilled).X * 0.5f), Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f + (font.MeasureString(textNumKilled).Y * 0.5f)),
                origin = Vector2.Zero

            });
            int exitTextId = AddEntity();
            string exitText = "Press Space to Exit";
            AddComponent<C2DRenderable>(exitTextId, new CText() {

                font = font,
                format = exitText,
                color = Color.Black,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f - (font.MeasureString(exitText).X * 0.5f), Game1.Inst.GraphicsDevice.Viewport.Height - (font.MeasureString(exitText).Y)),
                origin = Vector2.Zero

            });
        }

        public override void Update(float t, float dt)
        {
            passedTime += dt;
            if (passedTime > lifeTime || Keyboard.GetState().IsKeyDown(Keys.Space))
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
