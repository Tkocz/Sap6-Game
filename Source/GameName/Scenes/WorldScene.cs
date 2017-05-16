﻿using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Logging;
using EngineName.Systems;
using EngineName.Utils;
using GameName.Components;
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
        private int worldSize = 300;
        private int player;
        private int pickUpCount = 0;
        private List<CInventoryItem> itemsToAdd = new List<CInventoryItem>();
        private NetworkSystem _networkSystem;

        public WorldScene(NetworkSystem _network)
        {
            _networkSystem = _network;
        }
        public WorldScene() { }

        public override void Draw(float t, float dt)
        {
            Game1.Inst.GraphicsDevice.Clear(Color.Aqua);
            if (shouldLeave)
            {
                Game1.Inst.LeaveScene();
                Game1.Inst.EnterScene(new EndGameScene(passedTime, pickUpCount));
            }
            base.Draw(t, dt);
        }

        public override void Init()
        {
            var mapSystem = new MapSystem();
            var waterSys = new WaterSystem();
            var physicsSys = new PhysicsSystem();
            physicsSys.Bounds = new BoundingBox(-worldSize * Vector3.One, worldSize * Vector3.One);
            physicsSys.InvSpatPartSize = 0.1f;
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
                new AISystem(),
                new AnimationSystem(),
                new InventorySystem()
            );

#if DEBUG
            AddSystem(new DebugOverlay());
#endif

            base.Init();

            //add network after init

            if (_networkSystem != null)
            {
                AddSystem(_networkSystem);
                _networkSystem.InitLight();
            }

            // Camera entity
            int camera = AddEntity();
            float fieldofview = MathHelper.PiOver2;
            float nearplane = 0.1f;
            float farplane = 1000f;

            player = AddEntity();
            AddComponent(player, new CBody() {
                MaxVelocity = 5f,
                InvMass = 0.01f,
                SpeedMultiplier = 1,
                Radius = 1,
                Aabb = new BoundingBox(new Vector3(-1, -2, -1), new Vector3(1, 2, 1)),
                LinDrag = 0.8f,
                ReachableArea = new BoundingBox(new Vector3(-1.5f, -2, -1.5f), new Vector3(1.5f, 2, 1.5f)),
                Restitution = 0
            });
            AddComponent(player, new CInput());
            AddComponent(player, new CTransform() { Heading = MathHelper.PiOver2, Position = new Vector3(0, -0, 0), Scale = new Vector3(1f) });
            AddComponent<C3DRenderable>(player, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/viking") , fileName = "viking" });
            AddComponent(player, new CSyncObject());
            AddComponent(player, new CInventory());

            AddComponent(player, new CCamera(-50, 50)
            {
                Height = 20,
                Distance = 20,
                Projection = Matrix.CreatePerspectiveFieldOfView(fieldofview, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane, farplane)
                ,
                ClipProjection = Matrix.CreatePerspectiveFieldOfView(fieldofview * 1.2f, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane * 0.5f, farplane * 1.2f)
            });
            //AddComponent(camera, new CInput());
            //AddComponent(camera, new CTransform() { Position = new Vector3(-50, 50, 0), Rotation = Matrix.Identity, Scale = Vector3.One });


            // Heightmap entity
            int heightMap = AddEntity();
            AddComponent<C3DRenderable>(heightMap, new CHeightmap() { Image = Game1.Inst.Content.Load<Texture2D>("Textures/DinoIsland06") });
            AddComponent(heightMap, new CTransform() { Position = new Vector3(-590, 0, -590), Rotation = Matrix.Identity, Scale = new Vector3(1, 0.5f, 1) });
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
                     ((PhysicsSystem.CollisionInfo)data).Entity1 == sprintGoal))
                {
                    shouldLeave = true; // We reached the goal and wants to leave the scene-
                }
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

            CreateTriggerEvents();
            CreateCollectables();
            if ((_networkSystem != null && _networkSystem._isMaster) || _networkSystem == null)
            {
                CreateAnimals();
            }

            Log.GetLog().Debug("TestScene initialized.");
        }

        private void CreateCollectables() {
            int chests = 5, hearts = 5;
            for(int i = 0; i < chests; i++) {
                var id = AddEntity();
                AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "Models/chest", model = Game1.Inst.Content.Load<Model>("Models/chest") });
                var z = (float)(rnd.NextDouble() * worldSize);
                var x = (float)(rnd.NextDouble() * worldSize);
                AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(1f)});
                AddComponent(id, new CBody() { Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)) });
            }
            for(int i = 0; i < hearts; i++) {
                var id = AddEntity();
                AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "Models/heart", model = Game1.Inst.Content.Load<Model>("Models/heart") });
                var z = (float)(rnd.NextDouble() * worldSize);
                var x = (float)(rnd.NextDouble() * worldSize);
                AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(1f) });
                AddComponent(id, new CBody() { Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)) });
            }
        }

        private void CreateAnimals()
        {
            int flockCount = (int)(rnd.NextDouble() * 5) + 3;
            int membersPerFlock = (int)(rnd.NextDouble()*10) + 10;
            var flockRadius = membersPerFlock;
            for (int f = 0; f < flockCount; f++)
            {
                int flockId = AddEntity();
                CFlock flock = new CFlock {
                    Radius = 20,
                    SeparationDistance = 3,
                    AlignmentFactor = 0.1f,
                    CohesionFactor = 0.5f,
                    SeparationFactor = 100.0f,
                    PreferredMovementSpeed = 150f
                };

                double animal = rnd.NextDouble();
                string flockAnimal = animal > 0.66 ? "flossy" : animal > 0.33 ? "goose" : "hen";

                int flockX = (int)(rnd.NextDouble() * worldSize);
                int flockZ = (int)(rnd.NextDouble() * worldSize);
                CTransform flockTransform = new CTransform { Position = new Vector3(flockX, 0, flockZ) };
                flockTransform.Position += new Vector3(-worldSize / 2, 0, -worldSize / 2);

                for (int i = 0; i < membersPerFlock; i++) {
                    int id = AddEntity();
                    if(flockAnimal.Equals("hen")) {
                        // TODO: Make animals have different animations based on state
                        CAnimation normalAnimation = new CHenNormalAnimation();
                        // Set a random offset to animation so not all animals are synced
                        normalAnimation.CurrentKeyframe = rnd.Next(normalAnimation.Keyframes.Count-1);
                        AddComponent<C3DRenderable>(id, normalAnimation);
                    }
                    else {
                        CImportedModel modelComponent = new CImportedModel();
                        modelComponent.fileName = flockAnimal;
                        modelComponent.model = Game1.Inst.Content.Load<Model>("Models/" + modelComponent.fileName);
                        AddComponent<C3DRenderable>(id, modelComponent);
                    }

                    float memberX = flockTransform.Position.X + (float)rnd.NextDouble() * flockRadius * 2 - flockRadius;
                    float memberZ = flockTransform.Position.Z + (float)rnd.NextDouble() * flockRadius * 2 - flockRadius;
                    float y = flockTransform.Position.Y;
                    CTransform transformComponent = new CTransform();

                    transformComponent.Position = new Vector3(memberX, y, memberZ);
                    transformComponent.Rotation = Matrix.CreateFromAxisAngle(Vector3.UnitY,
                        (float)(Math.PI * (rnd.NextDouble() * 2)));
                    float scale = 1;
                    transformComponent.Scale = new Vector3(scale, scale, scale);
                    AddComponent(id, transformComponent);
                    AddComponent(id, new CBody
                    {
                        InvMass = 0.05f,
                        Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)),
                        LinDrag = 0.8f,
                        Velocity = Vector3.Zero,
                        Radius = 1f,
                        SpeedMultiplier = 0.5f,
                        MaxVelocity = 5,
                        Restitution = 0
                    });
                    AddComponent(id, new CAI { Flock = flockId });
                    AddComponent(id, new CSyncObject());

                    flock.Members.Add(id);
                }
                AddComponent(flockId, flock);
                AddComponent(flockId, flockTransform);
            }
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

            

            //var invComps = GetComponents<CInventory>();
            //foreach(var inv in invComps)
            //{
            //    var i = (CInventory)inv.Value;
            //    i.removeItems();
            //}
            base.Update(t, dt);
        }

        private int CreateBall(Vector3 p, Vector3 v, float r = 1.0f)
        {
            var ball = AddEntity();

            AddComponent(ball, new CBody
            {
                Aabb = new BoundingBox(-r * Vector3.One, r * Vector3.One),
                Radius = r,
                LinDrag = 0.2f,
                Velocity = v,
                Restitution = 0.3f
            });

            AddComponent(ball, new CTransform
            {
                Position = p,
                Rotation = Matrix.Identity,
                Scale = r * Vector3.One
            });
            //AddComponent(ball, new CSyncObject());
            AddComponent<C3DRenderable>(ball, new CImportedModel
            {
                model = Game1.Inst.Content.Load<Model>("Models/DummySphere"),
                fileName = "DummySphere"
            });
            Game1.Inst.Scene.AddComponent(ball, new CPickUp());
            return ball;
        }

        private void CreateTriggerEvents()
        {
            for (int i = 0; i < 40; i++)
            {
                int id = AddEntity();
                AddComponent(id, new CBody() { Radius = 5, Aabb = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), LinDrag = 0.8f });
                AddComponent(id, new CTransform() { Position = new Vector3(rnd.Next(-worldSize, worldSize), -0, rnd.Next(-worldSize, worldSize)), Scale = new Vector3(1f) });
                if (rnd.NextDouble() > 0.5)
                {
                    // Falling balls event
                    OnEvent("collision", data => {
                        if ((((PhysicsSystem.CollisionInfo)data).Entity1 == player &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == id)
                               ||
                            (((PhysicsSystem.CollisionInfo)data).Entity1 == id &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == player))
                        {
                            CTransform playerPosition = (CTransform)GetComponentFromEntity<CTransform>(player);
                            for (var j = 0; j < 6; j++)
                            {
                                var r = 0.6f + (float)rnd.NextDouble() * 2.0f;
                                var ballId = CreateBall(new Vector3((float)Math.Sin(j) * j + playerPosition.Position.X, playerPosition.Position.Y + 10f + 2.0f * j, (float)Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                           new Vector3(0.0f, -50.0f, 0.0f), // Velocity
                                           r);                              // Radius
                            }
                        }
                    });
                }
                else
                {
                    // Balls spawns around the player
                    OnEvent("collision", data => {
                        if ((((PhysicsSystem.CollisionInfo)data).Entity1 == player &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == id)
                               ||
                            (((PhysicsSystem.CollisionInfo)data).Entity1 == id &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == player))
                        {
                            CTransform playerPosition = (CTransform)GetComponentFromEntity<CTransform>(player);
                            for (var j = 0; j < 6; j++)
                            {
                                var r = 0.6f + (float)rnd.NextDouble() * 2.0f;
                                var ballId = CreateBall(new Vector3((float)Math.Sin(j) * j + playerPosition.Position.X, playerPosition.Position.Y + 2f, (float)Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                           new Vector3(0.0f, 0.0f, 0.0f), // Velocity
                                           r);                            // Radius
                            }
                        }
                    });
                }
            }
        }

        public void insertItemToAdd(CInventoryItem item)
        {
            itemsToAdd.Add(item);
        }

        public int GetPlayerEntityID()
        {

            return player;

        }
    }
}
