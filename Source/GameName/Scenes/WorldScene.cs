using Thengill;
using Thengill.Components;
using Thengill.Components.Renderable;
using Thengill.Core;
using Thengill.Logging;
using Thengill.Systems;
using Thengill.Utils;
using GameName.Components;
using GameName.Scenes.Utils;
using GameName.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Thengill.Shaders;

namespace GameName.Scenes
{
    public class WorldScene : Scene
    {
        private float passedTime = 0.0f;
        private bool shouldLeave = false;
        private Random rnd = new Random();
        private int worldSize = 590;
        private int heightMapScale = 300;
        private float yScaleMap = 0.4f;
        private int player;
        private int pickUpCount = 0;
        private bool won;

        private Hud mHud;

        private NetworkSystem _networkSystem;
        private WorldSceneConfig configs;
        private Effect mUnderWaterFx;
        private RenderTarget2D mRT;
        private RenderingSystem mRenderer;

        public WorldScene(WorldSceneConfig configs) {
            this.configs = configs;
            if(configs.network != null)
                _networkSystem = configs.network;
        }

        public override void Draw(float t, float dt)
        {
            if (shouldLeave) // TODO: When we parallelise this probably won't work.
            {
                Game1.Inst.LeaveScene();
                Game1.Inst.EnterScene(new EndGameScene(passedTime, pickUpCount,won));
            }

            var camera = (CCamera)GetComponentFromEntity<CCamera>(player);

            if (camera.Position.Y < configs.WaterHeight) {
                GfxUtil.SetRT(mRT);
                base.Draw(t, dt);
                GfxUtil.SetRT(null);
                mUnderWaterFx.Parameters["SrcTex"].SetValue(mRT);
                mUnderWaterFx.Parameters["Phase"].SetValue(t);
                GfxUtil.DrawFsQuad(mUnderWaterFx);
            }
            else {
                GfxUtil.SetRT(null);
                base.Draw(t, dt);

            }

            mHud.Update();
            mHud.Draw();
        }

        public void InitGameComponents()
        {
            Components.Add(typeof(CPlayer), new Dictionary<int, EcsComponent>());
        }
        private void InitSceneLightSettings()
        {
            DiffuseColor = new Vector3(1, 0.9607844f, 0.8078432f);
            Direction = new Vector3(-0.5265408f, - 0.5735765f, - 0.6275069f);
            SpecularColor = new Vector3(1, 0.9607844f, 0.8078432f);
            AmbientColor = new Vector3(0.05333332f, 0.09882354f, 0.1819608f);
        }
        public override void Init()
        {
            InitGameComponents();
            InitSceneLightSettings();
            
            mUnderWaterFx = Game1.Inst.Content.Load<Effect>("Effects/UnderWater");
            mRT = GfxUtil.CreateRT();
            
            int playerz = 0;
            int playerx = 0;
            if (configs.map == "Tropical") //"DinoIsland"
            {
                //waterSys.WaterHeight = -7;
                heightMapScale = 300;
                yScaleMap = 0.1f;
                playerz = 0;
                playerx = 0;
            }
            else if(configs.map == "UpNorth"){ //HeightMap
                heightMapScale = 300;
                yScaleMap = 0.5f;
                //waterSys.WaterHeight = -58;
                playerz = -49;
                playerx = -62;
            }

            var physicsSys = new PhysicsSystem();
            physicsSys.Bounds = new BoundingBox(-worldSize * Vector3.One, worldSize * Vector3.One);
            physicsSys.InvSpatPartSize = 0.07f;
            physicsSys.Gravity *= 2.0f;
            AddSystems(
                physicsSys,
                new InputSystem(),
                new AISystem(),
                new AnimationSystem(),
                new InventorySystem(),
                new CameraSystem(),
                new LogicSystem(),
    mRenderer = new RenderingSystem(),
                new Rendering2DSystem(),
                new HealthSystem()
            );

#if DEBUG
           AddSystem(new DebugOverlay());
#endif

#region REFACTOR THIS
           Func<float, float, float, Color> colorFn = (x, y, z) => {
               // The logic below is a bit messy - it's the result of some experimentation. Feel free
               // to tear it apart and come up with something better. :-) Basically, it computes and
               // interpolates between colors depending on the heightmap height at the given position.

               Func<float, float, float, float> f1 = (a, b, r) => (1.0f-r)*a + r*b;
               Func<Color, Color, float, Color> f = (a, b, r) =>
               new Color(f1(a.R/255.0f, b.R/255.0f, r),
                         f1(a.G/255.0f, b.G/255.0f, r),
                         f1(a.B/255.0f, b.B/255.0f, r),
                         f1(a.A/255.0f, b.A/255.0f, r));

               var r1 = 0.1f * (float)(rnd.NextDouble() - 0.5f);
               var r2 = 0.1f * (float)(rnd.NextDouble() - 0.5f);
               var r3 = 0.1f * (float)(rnd.NextDouble() - 0.5f);

               var rockColor  = new Color(0.6f+r1, 0.6f+r1, 0.65f+r1);
               var grassColor = new Color(0.2f+0.3f*r1, 0.4f+0.3f*r2, 0.3f+0.3f*r3);
               var sandColor  = new Color(0.3f+0.3f*r1, 0.1f+0.3f*r2, 0.0f+0.3f*r2);

               if (configs.map == "Tropical") {
                   rockColor = new Color(0.6f+0.3f*r1, 0.8f+0.3f*r2, 0.4f+0.3f*r3);
                   grassColor = new Color(0.35f+0.3f*r1, 0.55f+0.3f*r2, 0.25f+0.3f*r3);
                   sandColor = new Color(0.3f+0.3f*r1, 0.2f+0.3f*r2, 0.1f+0.3f*r3);
               }

               var color = grassColor;

               if (y < 0.0f) {
                   y += 0.4f;
                   var r = 2.0f/(1.0f + (float)Math.Pow(MathHelper.E, 40.0f*y)) - 1.0f;
                   r = Math.Max(Math.Min(r, 1.0f), 0.0f);
                   color = f(grassColor,
                             sandColor,
                             r);
               }

               if (y > 0.05f) {
                   y -= 0.05f;
                   var r = 2.0f/(1.0f + (float)Math.Pow(MathHelper.E, -90.0f*y)) - 1.0f;
                   r = Math.Max(Math.Min(r, 1.0f), 0.0f);
                   color = f(grassColor,
                             rockColor,
                             r);
               }

               return color;
           };
#endregion
            var heightmap = Heightmap.Load("Textures/" + configs.map,
                                           stepX      : 8,
                                           stepY      : 8,
                                           smooth     : false,
                                           scale      : heightMapScale,
                                           yScale     : yScaleMap,
                                           randomTris : true,
                                           blur       : 16,
                                           colorFn    : colorFn);

            physicsSys.Heightmap = heightmap;

            WaterFactory.Create(configs.WaterHeight, configs.TerrainWidth, configs.TerrainHeight);

            base.Init();

			SceneUtils.SpawnEnvironment(heightmap, heightMapScale);
            
            //add network after init
            if (_networkSystem != null)
            {

                var sync = new GameObjectSync(_networkSystem._isMaster);
                _networkSystem.InitLight();
                sync.Init();
                AddSystem(_networkSystem);
                AddSystem(sync);

            }

            // Camera entity
            float fieldofview = MathHelper.PiOver2;
            float nearplane = 0.1f;
            float farplane = 100f;

            player = AddEntity();

            Func<float, Matrix> playerAnim = (t) => {
                var transf = (CTransform)GetComponentFromEntity<CTransform>(player);
                var body = (CBody)GetComponentFromEntity<CBody>(player);

                // Wiggle wiggle!
                var x = 0.3f*Vector3.Dot(transf.Frame.Forward, body.Velocity);
                var walk =
                    Matrix.CreateFromAxisAngle(Vector3.Forward, x*0.1f*(float)Math.Cos(t*24.0f))
                  * Matrix.CreateTranslation(Vector3.Up * -x*0.1f*(float)Math.Sin(t*48.0f));

                var idle = Matrix.CreateTranslation(Vector3.Up * 0.07f*(float)Math.Sin(t*2.0f));

                return walk * idle;
            };

            AddComponent(player, new CBody() {
                MaxVelocity = 5f,
                InvMass = 0.01f,
                SpeedMultiplier = 1,
                Radius = 0.7f,
                Aabb = new BoundingBox(new Vector3(-0.5f, -0.9f, -0.5f), new Vector3(0.5f, 0.9f, 0.5f)),
                LinDrag = 0.8f,
                ReachableArea = new BoundingBox(new Vector3(-1.5f, -2.0f, -1.5f), new Vector3(1.5f, 2.0f, 1.5f)),
                Restitution = 0.1f
            });

            AddComponent(player, new CInput());
            AddComponent(player, new CPlayer());


            var playery = (heightmap.HeightAt(playerx, playerz));
            var playerTransf = new CTransform() { Heading = MathHelper.PiOver2, Position = new Vector3(playerx, playery, playerz), Scale = new Vector3(0.5f) };

            AddComponent(player, playerTransf);

            // Glossy helmet, lol!
            //var cubeMap = Game1.Inst.Content.Load<Effect>("Effects/CubeMap");
            //var envMap = new EnvMapMaterial(mRenderer, player, playerTransf, cubeMap);

            AddComponent<C3DRenderable>(player,
                                        new CImportedModel() {
                                            animFn = playerAnim,
                                            model = Game1.Inst.Content.Load<Model>("Models/viking") ,
                                            fileName = "viking",
                                            /*materials = new Dictionary<int, MaterialShader> {
                                                { 8, envMap }
                                            }*/
                                        });
            AddComponent(player, new CSyncObject { fileName = "viking" });

            AddComponent(player, new CInventory());
            AddComponent(player, new CHealth { MaxHealth = 3, Health = 3 });
            /*
            AddComponent(player, new CLogic {
                InvHz = 1.0f/30.0f,
                Fn = (t, dt) => {
                    var cam = (CCamera)GetComponentFromEntity<CCamera>(player);
                    envMap.Update();
                }
            });
            */
            AddComponent(player, new CCamera
            {
                Height = 3.5f,
                Distance = 3.5f,
                Projection = Matrix.CreatePerspectiveFieldOfView(fieldofview, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane, farplane)
                ,
                ClipProjection = Matrix.CreatePerspectiveFieldOfView(fieldofview * 1.2f, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane * 0.5f, farplane * 1.2f)
            });

            // Heightmap entity

            var mapMat = new ToonMaterial(Vector3.One*0.2f,
                                          new Vector3(1.0f, 0.0f, 1.0f), // ignored
                                          Vector3.Zero,
                                          40.0f,
                                          null, // diftex
                                          null, // normtex
                                          1.0f, // normcoeff
                                          5, // diflevels
                                          2, // spelevels,
                                          true); // use vert col


            int hme = AddEntity();
            AddComponent<C3DRenderable>(hme, new C3DRenderable { model = heightmap.Model,
                                                                 enableVertexColor = true,
                                                                 specular = 0.03f,
                // materials = new Dictionary<int, MaterialShader> { {0, mapMat } }
            });
            AddComponent(hme, new CTransform {
                Position = Vector3.Zero,
                Rotation = Matrix.Identity,
                Scale    = Vector3.One
            });

            int heightMap = AddEntity();
			var heightMapComp = new CHeightmap() { Image = Game1.Inst.Content.Load<Texture2D>("Textures/" + configs.map)};
			var heightTrans = new CTransform() { Position = new Vector3(-590, 0, -590), Rotation = Matrix.Identity, Scale = new Vector3(1, 0.5f, 1) };
            AddComponent<C3DRenderable>(heightMap, heightMapComp);
            AddComponent(heightMap, heightTrans);
			// manually start loading all heightmap components, should be moved/automated


            waterSys.Load();

           OnEvent("game_end", data =>
           {
                won = Game1.Inst.Scene.EntityHasComponent<CInput>((int) data);
                shouldLeave = true;
               // We reached the goal and wants to leave the scene-
           });

            //OnEvent("collision", data => {
            //    if (((PhysicsSystem.CollisionInfo)data).Entity1 == player &&
            //        balls.Contains(((PhysicsSystem.CollisionInfo)data).Entity2))
            //    {
            //        var inventory = (CInventory)GetComponentFromEntity<CInventory>(player);
            //        if (!inventory.isFull && !inventory.inventory.Contains(((PhysicsSystem.CollisionInfo)data).Entity2))
            //        {
            //            inventory.inventory.Add(((PhysicsSystem.CollisionInfo)data).Entity2);
            //            pickUpCount++;
            //        }
            //    }
            //    else if (balls.Contains(((PhysicsSystem.CollisionInfo)data).Entity1) &&
            //            ((PhysicsSystem.CollisionInfo)data).Entity2 == player)
            //    {
            //        var inventory = (CInventory)GetComponentFromEntity<CInventory>(player);
            //        if (!inventory.isFull && !inventory.inventory.Contains(((PhysicsSystem.CollisionInfo)data).Entity1))
            //        {
            //            inventory.inventory.Add(((PhysicsSystem.CollisionInfo)data).Entity1);
            //            pickUpCount++;
            //        }
            //    }
            //});


            if ((_networkSystem != null && _networkSystem._isMaster) || _networkSystem == null)
            {
                Utils.SceneUtils.CreateAnimals(configs.numFlocks, heightMapScale / 2);
                Utils.SceneUtils.CreateTriggerEvents(configs.numTriggers, heightMapScale / 2);
                Utils.SceneUtils.CreateCollectables(configs.numPowerUps, heightMapScale / 2);
                // Add tree as sprint goal
                int sprintGoal = AddEntity();
                AddComponent(sprintGoal, new CBody() { Radius = 5, Aabb = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), LinDrag = 0.8f });
                AddComponent(sprintGoal, new CTransform() { Position = new Vector3(100, -0, 100), Scale = new Vector3(1f) });
                var treefilename = "tree";
                AddComponent(sprintGoal, new CSyncObject());
                AddComponent<C3DRenderable>(sprintGoal, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/" + treefilename), fileName = treefilename });

                OnEvent("collision", data => {
                    foreach (var key in Game1.Inst.Scene.GetComponents<CPlayer>().Keys)
                    {
                        if ((((PhysicsSystem.CollisionInfo)data).Entity1 == key &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == sprintGoal)
                               ||
                            (((PhysicsSystem.CollisionInfo)data).Entity2 == key &&
                             ((PhysicsSystem.CollisionInfo)data).Entity1 == sprintGoal))
                        {
                            Game1.Inst.Scene.Raise("network_game_end", key);
                            Game1.Inst.Scene.Raise("game_end", key);
                        }
                    }
                });

            }

            Log.GetLog().Debug("WorldScene initialized.");

            InitHud();
        }

        public void InitHud() {
            mHud = new Hud();

            // Example of how to create hud elements.
            mHud.Button(10, 10, mHud.Text(() => "Click me (and check log)"))
                .OnClick(() => Console.WriteLine("Text button clicked."));

            mHud.Button(1050, 12, mHud.Sprite("Textures/Heart", 0.15f));
            mHud.Button(1100, 12, mHud.Sprite("Textures/Heart", 0.15f));
            mHud.Button(1150, 12, mHud.Sprite("Textures/Heart", 0.15f));
        }

        public override void Update(float t, float dt)
        {
            passedTime += dt;

            // TODO: Move to more appropriate location, only trying out heart rotation looks
            foreach(var comp in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                if (comp.Value.GetType() != typeof(CImportedModel))
                    continue;
                var modelComponent = (CImportedModel)comp.Value;
                if (modelComponent.fileName == null || !modelComponent.fileName.Contains("heart"))
                    continue;
                var transfComponent = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(comp.Key);
                transfComponent.Rotation *= Matrix.CreateFromAxisAngle(transfComponent.Frame.Up, dt);
            }

            base.Update(t, dt);
        }


        public int GetPlayerEntityID()
        {

            return player;

        }

        public int GetWorldSize() {
            return worldSize;
        }
    }
}
