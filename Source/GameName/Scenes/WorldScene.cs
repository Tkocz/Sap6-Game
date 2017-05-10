using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Logging;
using EngineName.Systems;
using EngineName.Utils;
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
        private int worldSize = 300;
        private int player;
        private NetworkSystem _networkSystem;

        public WorldScene(NetworkSystem _network)
        {
            _networkSystem = _network;
        }

        public override void Draw(float t, float dt) {
            Game1.Inst.GraphicsDevice.Clear(Color.Aqua);
            if (shouldLeave) {
                Game1.Inst.LeaveScene();
                Game1.Inst.EnterScene(new EndGameScene(passedTime));
            }
            base.Draw(t, dt);
        }

        public override void Init() {
            var mapSystem = new MapSystem();
            var waterSys = new WaterSystem();
            var physicsSys = new PhysicsSystem();
            physicsSys.Bounds = new BoundingBox(-worldSize * Vector3.One, worldSize * Vector3.One);
            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10),
                new SkyBoxSystem(),
                new RenderingSystem(),
                new CameraSystem(),
                physicsSys,
                mapSystem,
                new InputSystem(),
                waterSys,
                new Rendering2DSystem(),
                new AISystem()
            );

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

            base.Init();

            //add network after init
            AddSystem(_networkSystem);
            _networkSystem.InitLight();

            // Camera entity
            int camera = AddEntity();
            float fieldofview = MathHelper.PiOver2;
            float nearplane = 0.1f;
            float farplane = 1000f;

            player = AddEntity();
            AddComponent(player, new CBody() { MaxVelocity = 1f, InvMass = 0.1f, SpeedMultiplier = 1, Radius = 1, Aabb = new BoundingBox(new Vector3(-1, -2, -1), new Vector3(1, 2, 1)), LinDrag = 5f } );
            AddComponent(player, new CInput());
            AddComponent(player, new CTransform() { Position = new Vector3(0, -0, 0), Scale = new Vector3(1f) } );
            AddComponent<C3DRenderable>(player, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/viking") ,fileName  = "viking" });
			AddComponent(player,new CSyncObject());
			 
            AddComponent(camera, new CCamera(-50, 50) {
                Height = 20,
                Distance = 20,
                Projection = Matrix.CreatePerspectiveFieldOfView(fieldofview, Game1.Inst.GraphicsDevice.Viewport.AspectRatio,nearplane,farplane)
                ,ClipProjection = Matrix.CreatePerspectiveFieldOfView(fieldofview*1.2f, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane*0.5f, farplane*1.2f)
            });
            //AddComponent(camera, new CInput());
            AddComponent(camera, new CTransform() { Position = new Vector3(-50, 50, 0), Rotation = Matrix.Identity, Scale = Vector3.One });


            // Heightmap entity
            int heightMap = AddEntity();
            AddComponent<C3DRenderable>(heightMap, new CHeightmap() { Image = Game1.Inst.Content.Load<Texture2D>("Textures/Square_island_4x4") });
            AddComponent(heightMap, new CTransform() { Position = new Vector3(-590, -50, -590), Rotation = Matrix.Identity, Scale = new Vector3(1) });
            // manually start loading all heightmap components, should be moved/automated
            mapSystem.Load();
            waterSys.Load();
            physicsSys.MapSystem = mapSystem;
                       
            
            // Add tree as sprint goal

            int sprintGoal = AddEntity();
            //AddComponent(sprintGoal, new CTrigger());
            AddComponent(sprintGoal, new CBody() { Radius = 5, Aabb = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), LinDrag = 0.8f });
            AddComponent(sprintGoal, new CTransform() { Position = new Vector3(100, -0, 100), Scale = new Vector3(1f) });
            AddComponent<C3DRenderable>(sprintGoal, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/tree") });

            OnEvent("collision", data => {
                if ((((PhysicsSystem.CollisionInfo)data).Entity1 == player &&
                     ((PhysicsSystem.CollisionInfo)data).Entity2 == sprintGoal)
                       ||
                    (((PhysicsSystem.CollisionInfo)data).Entity2 == player &&
                     ((PhysicsSystem.CollisionInfo)data).Entity1 == sprintGoal)) {
                        shouldLeave = true; // We reached the goal and wants to leave the scene-
                }
            });

            CreateTriggerEvents(player);
            if ((_networkSystem != null && _networkSystem._isMaster) || _networkSystem == null)
            {
                CreateAnimals();
            }

            Log.Get().Debug("TestScene initialized.");
        }

        private void CreateAnimals() {
            var flockRadius = 10;
            for (int f = 0; f < 5; f++) {
                int flockId = AddEntity();
                CFlock flock = new CFlock();
                double animal = rnd.NextDouble();
                string flockAnimal = animal > 0.66 ? "flossy" : animal > 0.33 ? "goose" : "hen";
                int flockX = (int)(rnd.NextDouble() * worldSize);
                int flockZ = (int)(rnd.NextDouble() * worldSize);
                CTransform flockTransform = new CTransform { Position = new Vector3(flockX, 0, flockZ) };
                flockTransform.Position += new Vector3(-worldSize / 2, -50, -worldSize / 2);
                for (int i = 0; i < 10; i++) {
                    int id = AddEntity();
                    CImportedModel modelComponent = new CImportedModel();
                    modelComponent.fileName = flockAnimal;
                    modelComponent.model = Game1.Inst.Content.Load<Model>("Models/" + modelComponent.fileName);
                    AddComponent<C3DRenderable>(id, modelComponent);

                    float memberX = flockTransform.Position.X + (float)rnd.NextDouble() * flockRadius*2 - flockRadius;
                    float memberZ = flockTransform.Position.Z + (float)rnd.NextDouble() * flockRadius*2 - flockRadius;
                    float y = flockTransform.Position.Y;
                    CTransform transformComponent = new CTransform();

                    transformComponent.Position = new Vector3(memberX, y, memberZ);
                    //transformComponent.Position += new Vector3(-590, -50, -590);
                    transformComponent.Rotation = Matrix.CreateFromAxisAngle(Vector3.UnitY,
                        (float)(Math.PI * (rnd.NextDouble() * 2)));
                    float scale = 1;
                    transformComponent.Scale = new Vector3(scale, scale, scale);
                    AddComponent(id, transformComponent);
                    AddComponent(id, new CBody {
                        InvMass = 0.05f,
                        Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)),
                        LinDrag = 0.8f,
                        Velocity = Vector3.Zero,
                        Radius = 1f,
                        SpeedMultiplier = 0.5f,
                        MaxVelocity = 5
                    });
                    AddComponent(id, new CAI());
                    AddComponent(id, new CSyncObject());

                    flock.Members.Add(id);
                }
            }
        }

        public override void Update(float t, float dt) {
            passedTime += dt;

            Dictionary<int, EcsComponent> cameras = GetComponents<CCamera>();

            foreach(var camera in cameras) {
                CTransform cameraPos = (CTransform)GetComponentFromEntity<CTransform>(camera.Key);
                CTransform playerPos = (CTransform)GetComponentFromEntity<CTransform>(player);
                cameraPos.Position.X = playerPos.Position.X + ((CCamera)camera.Value).Distance;
                cameraPos.Position.Y = playerPos.Position.Y + ((CCamera)camera.Value).Height;
                cameraPos.Position.Z = playerPos.Position.Z + ((CCamera)camera.Value).Distance;
                ((CCamera)camera.Value).Target = playerPos.Position;
            } 

            base.Update(t, dt);
        }

        private int CreateBall(Vector3 p, Vector3 v, float r = 1.0f) {
            var ball = AddEntity();

            AddComponent(ball, new CBody {
                Aabb = new BoundingBox(-r * Vector3.One, r * Vector3.One),
                Radius = r,
                LinDrag = 0.2f,
                Velocity = v,
                Restitution = 0.3f
            });

            AddComponent(ball, new CTransform {
                Position = p,
                Rotation = Matrix.Identity,
                Scale = r * Vector3.One
            });
            //AddComponent(ball, new CSyncObject());
            AddComponent<C3DRenderable>(ball, new CImportedModel {
                model = Game1.Inst.Content.Load<Model>("Models/DummySphere"),fileName = "DummySphere" 
            });

            return ball;
        }

        private void CreateTriggerEvents(int playerID) {
            for (int i = 0; i < 40; i++) {
                int id = AddEntity();
                AddComponent(id, new CBody() { Radius = 5, Aabb = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), LinDrag = 0.8f });
                AddComponent(id, new CTransform() { Position = new Vector3(rnd.Next(-worldSize, worldSize), -0, rnd.Next(-worldSize, worldSize)), Scale = new Vector3(0.05f) });
                if (rnd.NextDouble() > 0.5) {
                    // Falling balls event
                    OnEvent("collision", data => {
                        if ((((PhysicsSystem.CollisionInfo)data).Entity1 == playerID &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == id)
                               ||
                            (((PhysicsSystem.CollisionInfo)data).Entity2 == id &&
                             ((PhysicsSystem.CollisionInfo)data).Entity1 == playerID)) {
                            CTransform playerPosition = (CTransform)GetComponentFromEntity<CTransform>(playerID);
                            for (var j = 0; j < 6; j++) {
                                var r = 0.6f + (float)rnd.NextDouble() * 2.0f;
                                CreateBall(new Vector3((float)Math.Sin(j) * j + playerPosition.Position.X, 10f + 2.0f * j, (float)Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                           new Vector3(0.0f, -50.0f, 0.0f), // Velocity
                                           r);                              // Radius
                            }
                        }
                    });
                } else {
                    // Balls spawns around the player
                    OnEvent("collision", data => {
                        if ((((PhysicsSystem.CollisionInfo)data).Entity1 == playerID &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == id)
                               ||
                            (((PhysicsSystem.CollisionInfo)data).Entity2 == id &&
                             ((PhysicsSystem.CollisionInfo)data).Entity1 == playerID)) {
                            CTransform playerPosition = (CTransform)GetComponentFromEntity<CTransform>(playerID);
                            for (var j = 0; j < 6; j++) {
                                var r = 0.6f + (float)rnd.NextDouble() * 2.0f;
                                CreateBall(new Vector3((float)Math.Sin(j) * j + playerPosition.Position.X, playerPosition.Position.Y + 2f, (float)Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                           new Vector3(0.0f, 0.0f, 0.0f), // Velocity
                                           r);                            // Radius
                            }
                        }
                    });
                }
            }
        }
    }
}
