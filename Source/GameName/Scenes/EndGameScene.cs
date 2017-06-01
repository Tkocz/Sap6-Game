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
        private const float lifeTime = 10.0f;
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
            int text = AddEntity();
            string winlos = won ? "You win" : "You lose";            
            winlos = string.Format("Game Over, {0}" +
                          " Number of animals killed: {1} \n", winlos, score);
            AddComponent<C2DRenderable>(text, new CText()
            {

                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans"),
                format = winlos,
                color = Color.Black,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f, Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f),
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
