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
        private int player;
        private int pickUpCount = 0;
        private bool won;
        private Hud mHud;
        private NetworkSystem _networkSystem;
        private WorldSceneConfig configs;
        private Effect mUnderWaterFx;
        private RenderTarget2D mRT;
        private RenderingSystem mRenderer;
        private ParticleSystem mParticleSys;
        private PostProcessor mPostProcessor;

        public WorldScene(WorldSceneConfig configs) {
            this.configs = configs;
            if(configs.Network != null)
                _networkSystem = configs.Network;
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
            else if (configs.IsRaining) {
                GfxUtil.SetRT(mRT);
                base.Draw(t, dt);
                GfxUtil.SetRT(null);
                mPostProcessor.ApplyPostProcess(t, dt, mRT);
            }
            else {
                GfxUtil.SetRT(null);
                base.Draw(t, dt);

            }

            mHud.Update();
            mHud.Draw(player);
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

            mPostProcessor = new PostProcessor();
            mUnderWaterFx = Game1.Inst.Content.Load<Effect>("Effects/UnderWater");
            mRT = GfxUtil.CreateRT();     

            var physicsSys = new PhysicsSystem();
            physicsSys.Bounds = new BoundingBox(-worldSize * Vector3.One, worldSize * Vector3.One);
            physicsSys.InvSpatPartSize = 0.07f;
            physicsSys.Gravity *= 2.0f;
            AddSystems(
                physicsSys,
                new InputSystem(),
                new AISystem(),
                new AnimationSystem(),
 mParticleSys = new ParticleSystem(),
                new InventorySystem(),
                new CameraSystem(),
                new LogicSystem(),
    mRenderer = new RenderingSystem(),
                new Rendering2DSystem(),
                new HealthSystem(),
                new HitSystem()
            );

#if DEBUG
           AddSystem(new DebugOverlay());
#endif

            var heightmap = Heightmap.Load("Textures/" + configs.Map,
                                           stepX      : 8,
                                           stepY      : 8,
                                           smooth     : false,
                                           scale      : configs.HeightMapScale,
                                           yScale     : configs.YScaleMap,
                                           randomTris : true,
                                           blur       : 16,
                                           colorFn    : configs.colorsMap);

            physicsSys.Heightmap = heightmap;

         
            base.Init();


            WaterFactory.Create(configs.WaterHeight, configs.HeightMapScale, configs.HeightMapScale);

            SceneUtils.SpawnEnvironment(heightmap, configs.HeightMapScale);
            
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


            var playery = (heightmap.HeightAt(configs.Playerx, configs.Playerz));
            var playerTransf = new CTransform() { Heading = MathHelper.PiOver2, Position = new Vector3(configs.Playerx, playery, configs.Playerz), Scale = new Vector3(0.5f) };

            AddComponent(player, playerTransf);

            // Glossy helmet, lol!
            //var cubeMap = Game1.Inst.Content.Load<Effect>("Effects/CubeMap");
            //var envMap = new EnvMapMaterial(mRenderer, player, playerTransf, cubeMap);

            AddComponent<C3DRenderable>(player,
                                        new CImportedModel() {
                                            animFn = SceneUtils.playerAnimation(player,24,0.1f),
                                            model = Game1.Inst.Content.Load<Model>("Models/viking") ,
                                            fileName = "viking",
                                            /*materials = new Dictionary<int, MaterialShader> {
                                                { 8, envMap }
                                            }*/
                                        });
            AddComponent(player, new CSyncObject { fileName = "viking" });

            AddComponent(player, new CInventory());
            AddComponent(player, new CHealth { MaxHealth = 3, Health = 3 });
			AddComponent(player, new CScore { });
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
			var heightMapComp = new CHeightmap() { Image = Game1.Inst.Content.Load<Texture2D>("Textures/" + configs.Map)};
			var heightTrans = new CTransform() { Position = new Vector3(-590, 0, -590), Rotation = Matrix.Identity, Scale = new Vector3(1, 0.5f, 1) };
            AddComponent<C3DRenderable>(heightMap, heightMapComp);
            AddComponent(heightMap, heightTrans);
            // manually start loading all heightmap components, should be moved/automated

            OnEvent("hit", data => {
                var key = data as int?;
                if (key == null) return;
                var transform = (CTransform)GetComponentFromEntity<CTransform>(key.Value);
                var id = AddEntity();
                Func<float> rndSize = () => 0.05f + 0.1f * (float)rnd.NextDouble();
                Func<Vector3> rndVel = () => new Vector3((float)rnd.NextDouble() - 0.5f,
                                                         (float)rnd.NextDouble(),
                                                         (float)rnd.NextDouble() - 0.5f);
                mParticleSys.SpawnParticles(50, () => new EcsComponent[] {
                    new CParticle     { Position = transform.Position,
                                        Velocity = 6.0f*rndVel(),
                                        Life     = 1.7f,
                                        F        = () => new Vector3(0.0f, -9.81f, 0.0f) },
                    new C3DRenderable { model = Game1.Inst.Content.Load<Model>("Models/blood") },
                    new CTransform    { Position = transform.Position,
                                        Rotation = Matrix.Identity,
                                        Scale    = rndSize()*Vector3.One } });
            });

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
                Utils.SceneUtils.CreateAnimals(configs.NumFlocks, configs.HeightMapScale / 2);
                Utils.SceneUtils.CreateTriggerEvents(configs.NumTriggers, configs.HeightMapScale / 2);
                Utils.SceneUtils.CreateCollectables(configs.NumPowerUps, configs.HeightMapScale / 2);
                SceneUtils.SpawnBirds(configs);
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

            var billboards = new [] {
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Grass", 1.0f),
                new Tuple<string, float>("Bush", 1.2f),
                new Tuple<string, float>("Flowers", 0.6f)
            };

            var billboards2 = new [] {
                new Tuple<string, float>("Seaweed1", 0.6f),
                new Tuple<string, float>("Seaweed2", 0.6f),
            };
;
            for (var i = 0; i < 10000; i++) {
                var bbs = billboards;

                var x = configs.HeightMapScale * ((float)rnd.NextDouble() - 0.5f);
                var z = configs.HeightMapScale * ((float)rnd.NextDouble() - 0.5f);
                var y = heightmap.HeightAt(x, z);

                if (y < configs.WaterHeight) {
                    bbs = billboards2;
                }

                var bb = bbs[rnd.Next(0, bbs.Length)];
                var s = (1.0f + 0.8f*(float)rnd.NextDouble()) * bb.Item2;

                AddComponent(AddEntity(), new CBillboard {
                    Pos   = new Vector3(x, y + 0.5f*s , z),
                    Tex   = Game1.Inst.Content.Load<Texture2D>("Textures/" + bb.Item1),
                    Scale = s
                });
            }

            CreatePlatforms(heightmap);

        }

        public void CreatePlatform(Heightmap heightmap, float x1, float z1, float x2, float z2) {
            var y1 = heightmap.HeightAt(x1, z1);
            var y2 = heightmap.HeightAt(x2, z2);

            if (y1 < configs.WaterHeight) y1 = configs.WaterHeight;
            if (y2 < configs.WaterHeight) y2 = configs.WaterHeight;

            var a = new Vector3(x1, y1+0.5f, z1);
            var b = new Vector3(x2, y2+0.5f, z2);

            var d1 = b - a;
            var d2 = d1;
            var d3 = d2;

            d2.Y = 0.0f;

            d2.Normalize();
            d3.Normalize();

            var theta1 = (float)Math.Acos(Vector3.Dot(Vector3.Forward, d2));
            var axis1  = Vector3.Cross(Vector3.Forward, d2);
            var rot1   = Matrix.CreateFromAxisAngle(axis1, theta1);

            var theta2 = (float)Math.Acos(Vector3.Dot(d2, d3));
            var axis2  = Vector3.Cross(d2, d3);
            var rot2   = Matrix.CreateFromAxisAngle(axis2, theta2);

            var rot = rot1 * rot2;

            var plat = AddEntity();
            AddComponent<C3DRenderable>(plat,
                                        new C3DRenderable {
                                            model = Game1.Inst.Content.Load<Model>("Models/Platform1"),
                                        });

            AddComponent<CTransform>(plat,
                                     new CTransform {
                                         Position = a + 0.5f*d1,
                                         Rotation = rot,
                                         Scale = new Vector3(1.6f, 1.0f, d1.Length())
                                     });

            AddComponent<CBox>(plat,
                               new CBox {
                                   Box = new BoundingBox(new Vector3(-1.6f, -0.2f, -d1.Length()),
                                                         new Vector3( 1.6f,  0.2f,  d1.Length())),
                                   InvTransf = Matrix.Invert(rot)
                               });
        }

        public void CreatePlatforms(Heightmap heightmap) {
            for (var i = 0; i < 20; i++) {
                var x1 = 0.8f*configs.HeightMapScale * ((float)rnd.NextDouble() - 0.5f);
                var z1 = 0.8f*configs.HeightMapScale * ((float)rnd.NextDouble() - 0.5f);

                var x2 = x1 + 15.0f * ((float)rnd.NextDouble() - 0.5f);
                var z2 = z1 + 15.0f * ((float)rnd.NextDouble() - 0.5f);

                CreatePlatform(heightmap, x1, z1, x2, z2);
            }
        }

        public void InitHud() {
            mHud = new Hud();
            // Example of how to create hud elements.
            mHud.Button("Click me",10, 10, mHud.Text(() => "Click me (and check log)"))
                .OnClick(() => Console.WriteLine("Text button clicked."));
            var heart = (CHealth) Game1.Inst.Scene.GetComponentFromEntity<CHealth>(player);
            for (int i = 0; i <heart.Health; i++)
            {
                mHud.Button("heart"+i, 1050 + i*(50), 12, mHud.Sprite("Textures/Heart", 0.15f));
            }
         
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
    }
}
