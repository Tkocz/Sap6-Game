using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Logging;
using EngineName.Systems;
using EngineName.Utils;
using GameName.Components;
using GameName.Scenes.Utils;
using GameName.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        private List<int> balls = new List<int>();
        private NetworkSystem _networkSystem;
        private WorldSceneConfig configs;

        public WorldScene(WorldSceneConfig configs) {
            this.configs = configs;
            if(configs.network != null)
                _networkSystem = configs.network;
        }

        public override void Draw(float t, float dt)
        {
            Game1.Inst.GraphicsDevice.Clear(new Color(0.4f, 0.6f, 0.8f));
            if (shouldLeave) // TODO: When we parallelise this probably won't work.
            {
                Game1.Inst.LeaveScene();
                Game1.Inst.EnterScene(new EndGameScene(passedTime, pickUpCount,won));
            }
            base.Draw(t, dt);
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

            var waterSys = new WaterSystem();
            var physicsSys = new PhysicsSystem();
            physicsSys.Bounds = new BoundingBox(-worldSize * Vector3.One, worldSize * Vector3.One);
            physicsSys.InvSpatPartSize = 0.07f;
            AddSystems(
                new RenderingSystem(),
                new CameraSystem(),
                physicsSys,
                new InputSystem(),
                waterSys,
                new Rendering2DSystem(),
                new AISystem(),
                new AnimationSystem(),
                new InventorySystem()
            );

#if DEBUG
           AddSystem(new DebugOverlay());
#endif

            var heightmap = Heightmap.Load("Textures/HeightMap",
                                           stepX: 16,
                                           stepY: 16,
                                           smooth: false,
                                           scale: 200.0f,
                                           yScale: 0.3f);
            physicsSys.Heightmap = heightmap;

            base.Init();

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
            float farplane = 1000f;

            player = AddEntity();
            AddComponent(player, new CBody() {
                MaxVelocity = 5f,
                InvMass = 0.01f,
                SpeedMultiplier = 1,
                Radius = 1,
                Aabb = new BoundingBox(new Vector3(-0.5f, -0.9f, -0.5f), new Vector3(0.5f, 0.9f, 0.5f)),
                LinDrag = 50f,
                ReachableArea = new BoundingBox(new Vector3(-1.5f, -2.0f, -1.5f), new Vector3(1.5f, 2.0f, 1.5f)),
                Restitution = 0.4f
            });
            AddComponent(player, new CInput());
            AddComponent(player, new CPlayer());
            AddComponent(player, new CTransform() { Heading = MathHelper.PiOver2, Position = new Vector3(0, -0, rnd.Next(0,50)), Scale = new Vector3(0.5f) });
            AddComponent<C3DRenderable>(player, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/viking") , fileName = "viking" });
            AddComponent(player, new CSyncObject { fileName = "viking" });
            AddComponent(player, new CInventory());

            AddComponent(player, new CCamera(-50, 50)
            {
                Height = 5,
                Distance = 5,
                Projection = Matrix.CreatePerspectiveFieldOfView(fieldofview, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane, farplane)
                ,
                ClipProjection = Matrix.CreatePerspectiveFieldOfView(fieldofview * 1.2f, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane * 0.5f, farplane * 1.2f)
            });

            // Heightmap entity

            int hme = AddEntity();
            AddComponent<C3DRenderable>(hme, new C3DRenderable { model = heightmap.Model,
                                                                 enableVertexColor = true,
                                                                 specular = 0.0f });
            AddComponent(hme, new CTransform {
                Position = Vector3.Zero,
                Rotation = Matrix.Identity,
                Scale    = Vector3.One
            });

            int heightMap = AddEntity();
			Dictionary<int, string> elementList = new Dictionary<int, string>();
			elementList.Add(255, "LeafTree");
			elementList.Add(245, "PalmTree");
			elementList.Add(235, "tree");
			elementList.Add(170, "rock2");
			var heightMapComp = new CHeightmap() { Image = Game1.Inst.Content.Load<Texture2D>("Textures/" + configs.map), elements = elementList };
			var heightTrans = new CTransform() { Position = new Vector3(-590, 0, -590), Rotation = Matrix.Identity, Scale = new Vector3(1, 0.5f, 1) };
            AddComponent<C3DRenderable>(heightMap, heightMapComp);
            AddComponent(heightMap, heightTrans);
            // manually start loading all heightmap components, should be moved/automated

			foreach (var i in heightMapComp.EnvironmentSpawn )
			{
				int newElement = Game1.Inst.Scene.AddEntity ();
				Game1.Inst.Scene.AddComponent(newElement, new CBox() { Box = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), InvTransf = Matrix.Identity });
				Game1.Inst.Scene.AddComponent(newElement, new CTransform() { Position = new Vector3(i.X + heightTrans.Position.X, i.Y * heightTrans.Scale.Y - 1f, i.Z + heightTrans.Position.Z), Scale = new Vector3((float)rnd.NextDouble() * 0.25f + 0.75f), Rotation = Matrix.CreateRotationY((float)rnd.NextDouble() * MathHelper.Pi * 2f) });
				Game1.Inst.Scene.AddComponent<C3DRenderable>(newElement, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/" + heightMapComp.elements[(int)i.W])  ,fileName = heightMapComp.elements[(int)i.W] });
			}
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
                Utils.SceneUtils.CreateAnimals(configs.numFlocks);
                Utils.SceneUtils.CreateTriggerEvents(configs.numTriggers);
                Utils.SceneUtils.CreateCollectables(configs.numPowerUps);
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

            Log.GetLog().Debug("TestScene initialized.");
        }

        public override void Update(float t, float dt)
        {
            passedTime += dt;

            Dictionary<int, EcsComponent> cameras = GetComponents<CCamera>();

            /*foreach (var camera in cameras) {
                CTransform cameraPos = (CTransform)GetComponentFromEntity<CTransform>(camera.Key);
                CTransform playerPos = (CTransform)GetComponentFromEntity<CTransform>(player);
                cameraPos.Position.X = playerPos.Position.X + ((CCamera)camera.Value).Distance;
                cameraPos.Position.Y = playerPos.Position.Y + ((CCamera)camera.Value).Height;
                cameraPos.Position.Z = playerPos.Position.Z + ((CCamera)camera.Value).Distance;
                //((CCamera)camera.Value).Target = playerPos.Position;
                //((CCamera)camera.Value).Heading += 0.1f;
            }*/
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
