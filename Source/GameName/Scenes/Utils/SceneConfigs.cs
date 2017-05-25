using Thengill.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Thengill.Utils;

namespace GameName.Scenes.Utils {
    public class WorldSceneConfig
    {
        public bool IsRaining;
        public int NumFlocks;
        public int NumPowerUps;
        public int NumTriggers;
        public string Map;
        public float WaterHeight;
        public int HeightMapScale = 300;
        public float YScaleMap = 0.4f;
        public NetworkSystem Network;
        public int Playerz = 0;
        public int Playerx = 0;
        private Random rnd = new Random();
        public Func<float, float, float, Color> colorsMap;
        public LightingConfig LightConfig;

        public WorldSceneConfig(int numFlocks, int numPowerUps, int numTriggers, string map, NetworkSystem network)
        {
            NumFlocks = numFlocks;
            NumPowerUps = numPowerUps;
            NumTriggers = numTriggers;
            Map = map;
            Network = network;
            Map = map;

            LightConfig = new LightingConfig();
            LightConfig.DiffuseColor = new Vector3(1, 0.9607844f, 0.8078432f);
            LightConfig.Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f);
            LightConfig.SpecularColor = new Vector3(1, 0.9607844f, 0.8078432f);
            LightConfig.AmbientColor = new Vector3(0.05333332f, 0.09882354f, 0.1819608f);

            if (Map == "Tropical") //"DinoIsland"
            {
                IsRaining = false;
                WaterHeight = -7;
                HeightMapScale = 300;
                YScaleMap = 0.1f;
                Playerz = 0;
                Playerx = 0;
                colorsMap = createColorsTropical();
            }
            else if (Map == "UpNorth")
            {
                IsRaining = true;
                //HeightMap
                HeightMapScale = 300;
                YScaleMap = 0.5f;
                WaterHeight = -58;
                Playerz = -49;
                Playerx = -62;
                colorsMap = createColors();
            }
        }

        public Func<float, float, float, Color> createColorsTropical()
        {
            Func<float, float, float, Color> colorFn = (x, y, z) =>
            {
                // The logic below is a bit messy - it's the result of some experimentation. Feel free
                // to tear it apart and come up with something better. :-) Basically, it computes and
                // interpolates between colors depending on the heightmap height at the given position.

                Func<float, float, float, float> f1 = (a, b, r) => (1.0f - r) * a + r * b;
                Func<Color, Color, float, Color> f = (a, b, r) =>
                    new Color(f1(a.R / 255.0f, b.R / 255.0f, r),
                        f1(a.G / 255.0f, b.G / 255.0f, r),
                        f1(a.B / 255.0f, b.B / 255.0f, r),
                        f1(a.A / 255.0f, b.A / 255.0f, r));

                var r1 = 0.1f * (float) (rnd.NextDouble() - 0.5f);
                var r2 = 0.1f * (float) (rnd.NextDouble() - 0.5f);
                var r3 = 0.1f * (float) (rnd.NextDouble() - 0.5f);

                var rockColor  = new Color(0.6f + r1, 0.6f + r1, 0.65f + r1);
                var grassColor = new Color(0.2f + 0.3f * r1, 0.4f + 0.3f * r2, 0.3f + 0.3f * r3);
                var sandColor  = new Color(0.3f + 0.3f * r1, 0.1f + 0.3f * r2, 0.0f + 0.3f * r2);

                if (Map == "Tropical")
                {
                    grassColor  = new Color(0.9f + 0.3f * r1, 0.85f + 0.3f * r2, 0.55f + 0.3f * r3);
                    rockColor = new Color(0.2f + 0.3f * r1, 0.5f + 0.3f * r2, 0.1f + 0.3f * r3);
                    sandColor  = new Color(0.2f + r1, 0.2f + r1, 0.2f + r1);
                }

                var color = grassColor;

                if (y < -0.3) {
                    color = new Color(0.4f + 0.3f * r1, 0.35f + 0.3f * r2, 0.1f + 0.3f * r3);
                }
                else if (y > 0.05) {
                    color = new Color(0.4f + 0.3f * r1, 0.7f + 0.3f * r2, 0.3f + 0.3f * r3);;
                }
                else if (y > -0.05) {
                    color = rockColor;
                }
                else if (y > -0.2) {
                    color = grassColor;
                }
                else if (y > -0.3) {
                    color = new Color(0.7f + 0.3f * r1, 0.65f + 0.3f * r2, 0.5f + 0.3f * r3);
                }


                return color;
            };
            return colorFn;
        }

        public Func<float, float, float, Color> createColors()
        {
            Func<float, float, float, Color> colorFn = (x, y, z) =>
            {
                // The logic below is a bit messy - it's the result of some experimentation. Feel free
                // to tear it apart and come up with something better. :-) Basically, it computes and
                // interpolates between colors depending on the heightmap height at the given position.

                Func<float, float, float, float> f1 = (a, b, r) => (1.0f - r) * a + r * b;
                Func<Color, Color, float, Color> f = (a, b, r) =>
                    new Color(f1(a.R / 255.0f, b.R / 255.0f, r),
                        f1(a.G / 255.0f, b.G / 255.0f, r),
                        f1(a.B / 255.0f, b.B / 255.0f, r),
                        f1(a.A / 255.0f, b.A / 255.0f, r));

                var r1 = 0.1f * (float) (rnd.NextDouble() - 0.5f);
                var r2 = 0.1f * (float) (rnd.NextDouble() - 0.5f);
                var r3 = 0.1f * (float) (rnd.NextDouble() - 0.5f);

                var rockColor = new Color(0.6f + r1, 0.6f + r1, 0.65f + r1);
                var grassColor = new Color(0.2f + 0.3f * r1, 0.4f + 0.3f * r2, 0.3f + 0.3f * r3);
                var sandColor = new Color(0.3f + 0.3f * r1, 0.1f + 0.3f * r2, 0.0f + 0.3f * r2);

                if (Map == "Tropical")
                {
                    rockColor = new Color(0.6f + 0.3f * r1, 0.8f + 0.3f * r2, 0.4f + 0.3f * r3);
                    grassColor = new Color(0.35f + 0.3f * r1, 0.55f + 0.3f * r2, 0.25f + 0.3f * r3);
                    sandColor = new Color(0.3f + 0.3f * r1, 0.2f + 0.3f * r2, 0.1f + 0.3f * r3);
                }

                var color = grassColor;

                if (y < 0.0f)
                {
                    y += 0.4f;
                    var r = 2.0f / (1.0f + (float) Math.Pow(MathHelper.E, 40.0f * y)) - 1.0f;
                    r = Math.Max(Math.Min(r, 1.0f), 0.0f);
                    color = f(grassColor,
                        sandColor,
                        r);
                }

                if (y > 0.05f)
                {
                    y -= 0.05f;
                    var r = 2.0f / (1.0f + (float) Math.Pow(MathHelper.E, -90.0f * y)) - 1.0f;
                    r = Math.Max(Math.Min(r, 1.0f), 0.0f);
                    color = f(grassColor,
                        rockColor,
                        r);
                }

                return color;
            };
            return colorFn;
        }
    }
}
