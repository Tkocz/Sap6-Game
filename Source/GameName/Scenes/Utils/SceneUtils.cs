using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Systems;
using GameName.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Scenes.Utils {
    class SceneUtils {
        private static Random rnd = new Random();

        public static int CreateBall(Vector3 p, Vector3 v, float r = 1.0f) {
            var currentScene = Game1.Inst.Scene;
            var ball = currentScene.AddEntity();

            currentScene.AddComponent(ball, new CBody {
                Aabb = new BoundingBox(-r * Vector3.One, r * Vector3.One),
                Radius = r,
                LinDrag = 0.2f,
                Velocity = v,
                Restitution = 0.3f
            });

            currentScene.AddComponent(ball, new CTransform {
                Position = p,
                Rotation = Matrix.Identity,
                Scale = r * Vector3.One
            });
            currentScene.AddComponent(ball, new CSyncObject());
            currentScene.AddComponent<C3DRenderable>(ball, new CImportedModel {
                model = Game1.Inst.Content.Load<Model>("Models/DummySphere"),
                fileName = "DummySphere"
            });

            return ball;
        }

        public static void CreateAnimals(int numFlocks) {
            var currentScene = Game1.Inst.Scene;
            
            int flockCount = (int)(rnd.NextDouble() * 5) + 3;
            int membersPerFlock = (int)(rnd.NextDouble() * 10) + 10;
            var flockRadius = membersPerFlock;
            for (int f = 0; f < numFlocks; f++) {
                int flockId = currentScene.AddEntity();
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

                // TODO: get the global value of the worldsize
                int flockX = (int)(rnd.NextDouble() * 300);// HERE
                int flockZ = (int)(rnd.NextDouble() * 300);// HERE
                CTransform flockTransform = new CTransform { Position = new Vector3(flockX, 0, flockZ) };
                flockTransform.Position += new Vector3(-300 / 2, 0, -300 / 2);// HERE

                for (int i = 0; i < membersPerFlock; i++) {
                    int id = currentScene.AddEntity();
                    if (flockAnimal.Equals("hen")) {
                        // TODO: Make animals have different animations based on state
                        CAnimation normalAnimation = new CHenNormalAnimation();
                        // Set a random offset to animation so not all animals are synced
                        normalAnimation.CurrentKeyframe = rnd.Next(normalAnimation.Keyframes.Count - 1);
                        currentScene.AddComponent<C3DRenderable>(id, normalAnimation);
                    } else {
                        CImportedModel modelComponent = new CImportedModel();
                        modelComponent.fileName = flockAnimal;
                        modelComponent.model = Game1.Inst.Content.Load<Model>("Models/" + modelComponent.fileName);
                        currentScene.AddComponent<C3DRenderable>(id, modelComponent);
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
                    currentScene.AddComponent(id, transformComponent);
                    currentScene.AddComponent(id, new CBody {
                        InvMass = 0.05f,
                        Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)),
                        LinDrag = 0.8f,
                        Velocity = Vector3.Zero,
                        Radius = 1f,
                        SpeedMultiplier = 0.5f,
                        MaxVelocity = 5,
                        Restitution = 0
                    });
                    currentScene.AddComponent(id, new CAI { Flock = flockId });
                    currentScene.AddComponent(id, new CSyncObject());
                    currentScene.AddComponent(id, new CSyncObject());

                    flock.Members.Add(id);
                }
                currentScene.AddComponent(flockId, flock);
                currentScene.AddComponent(flockId, flockTransform);
            }
        }

        public static void CreateCollectables(int numPowerUps) {
            var currentScene = Game1.Inst.Scene;
            
            // TODO: get the global value of the worldsize
            int chests = numPowerUps, hearts = numPowerUps;
            for (int i = 0; i < chests; i++) {
                var id = currentScene.AddEntity();
                currentScene.AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "Models/chest", model = Game1.Inst.Content.Load<Model>("Models/chest") });
                var z = (float)(rnd.NextDouble() * 300);// HERE
                var x = (float)(rnd.NextDouble() * 300);// HERE
                currentScene.AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(1f) });
                currentScene.AddComponent(id, new CBody() { Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)) });
            }
            for (int i = 0; i < hearts; i++) {
                var id = currentScene.AddEntity();
                currentScene.AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "Models/heart", model = Game1.Inst.Content.Load<Model>("Models/heart") });
                var z = (float)(rnd.NextDouble() * 300);// HERE
                var x = (float)(rnd.NextDouble() * 300);// HERE
                currentScene.AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(1f) });
                currentScene.AddComponent(id, new CBody() { Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)) });
            }
        }
        
        public static void CreateTriggerEvents(int player, int numTriggers) {
            var currentScene = Game1.Inst.Scene;
            Random rnd = new Random();
            
            // TODO: get the global value of the worldsize
            for (int i = 0; i < numTriggers; i++) {
                int id = currentScene.AddEntity();
                currentScene.AddComponent(id, new CBody() { Radius = 5, Aabb = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), LinDrag = 0.8f });
                currentScene.AddComponent(id, new CTransform() { Position = new Vector3(rnd.Next(-300, 300), -0, // HERE
                                                                                        rnd.Next(-300, 300)),// HERE
                                                                                        Scale = new Vector3(1f) });
                if (rnd.NextDouble() > 0.5) {
                    // Falling balls event
                    currentScene.OnEvent("collision", data => {
                        if ((((PhysicsSystem.CollisionInfo)data).Entity1 == player &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == id)
                               ||
                            (((PhysicsSystem.CollisionInfo)data).Entity1 == id &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == player)) {
                            CTransform playerPosition = (CTransform)currentScene.GetComponentFromEntity<CTransform>(player);
                            for (var j = 0; j < 6; j++) {
                                var r = 0.6f + (float)rnd.NextDouble() * 2.0f;
                                var ballId = Utils.SceneUtils.CreateBall(new Vector3((float)Math.Sin(j) * j + playerPosition.Position.X, playerPosition.Position.Y + 10f + 2.0f * j, (float)Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                           new Vector3(0.0f, -50.0f, 0.0f), // Velocity
                                           r);                              // Radius
                                //balls.Add(ballId);
                            }
                        }
                    });
                } else {
                    // Balls spawns around the player
                    currentScene.OnEvent("collision", data => {
                        if ((((PhysicsSystem.CollisionInfo)data).Entity1 == player &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == id)
                               ||
                            (((PhysicsSystem.CollisionInfo)data).Entity1 == id &&
                             ((PhysicsSystem.CollisionInfo)data).Entity2 == player)) {
                            CTransform playerPosition = (CTransform)currentScene.GetComponentFromEntity<CTransform>(player);
                            for (var j = 0; j < 6; j++) {
                                var r = 0.6f + (float)rnd.NextDouble() * 2.0f;
                                var ballId = Utils.SceneUtils.CreateBall(new Vector3((float)Math.Sin(j) * j + playerPosition.Position.X, playerPosition.Position.Y + 2f, (float)Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                           new Vector3(0.0f, 0.0f, 0.0f), // Velocity
                                           r);                            // Radius
                                //balls.Add(ballId);
                            }
                        }
                    });
                }
            }
        }
    }
}
