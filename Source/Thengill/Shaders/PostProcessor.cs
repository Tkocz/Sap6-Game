using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Shaders
{
    public class PostProcessor
    {
        private Effect postprocessEffect;
        Texture2D sketchTexture;
        SpriteBatch spriteBatch;
        Vector2 sketchJitter;
        float timeToNextJitter;
        Random random = new Random();
        PostEffectSettings Settings;

        public PostProcessor()
        {
            spriteBatch = new SpriteBatch(Game1.Inst.GraphicsDevice);
            postprocessEffect = Game1.Inst.Content.Load<Effect>("Effects/PostprocessRain");
            sketchTexture = Game1.Inst.Content.Load<Texture2D>("Textures/Rain");
            Settings = new PostEffectSettings();

        }
        public void ApplyPostProcess(float t, float dt, RenderTarget2D rt)
        {
            updateSketchJitterSpeed(t, dt);

            EffectParameterCollection parameters = postprocessEffect.Parameters;
            string effectTechniqueName = "SketchRain";

            parameters["SketchThreshold"].SetValue(0.1f);
            parameters["SketchBrightness"].SetValue(0.333f);
            
            parameters["SketchThreshold"].SetValue(Settings.SketchThreshold);
            parameters["SketchBrightness"].SetValue(Settings.SketchBrightness);
            parameters["SketchJitter"].SetValue(sketchJitter);
            parameters["SketchTexture"].SetValue(sketchTexture);
            
            // Activate the appropriate effect technique.
            postprocessEffect.CurrentTechnique = postprocessEffect.Techniques[effectTechniqueName];

            // Draw a fullscreen sprite to apply the postprocessing effect.
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, postprocessEffect);
            spriteBatch.Draw(rt, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
        public class PostEffectSettings
        {

            // Settings for the pencil sketch effect.
            public readonly float SketchThreshold;
            public readonly float SketchBrightness;
            public readonly float SketchJitterSpeed;

            public PostEffectSettings()
            {
                SketchThreshold = 0.2f;
                SketchBrightness = 0.5f;
                SketchJitterSpeed = 0.075f;
            }
        }
        private void updateSketchJitterSpeed(float t, float dt)
        {
            if (Settings.SketchJitterSpeed > 0)
            {
                timeToNextJitter -= t;

                if (timeToNextJitter <= 0)
                {
                    sketchJitter.X = (float)random.NextDouble();
                    sketchJitter.Y = (float)random.NextDouble();

                    timeToNextJitter += Settings.SketchJitterSpeed;
                }
            }
        }
    }
}
