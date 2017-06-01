using Thengill;
using Thengill.Components;
using Thengill.Components.Renderable;
using Thengill.Systems;
using GameName.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thengill.Shaders;

namespace GameName.Scenes.Utils {
    class SceneUtils {
        private static Random rnd = new Random();

        public static int CreateBall(Vector3 p, Vector3 v, float r = 1.0f) {
            var currentScene = Game1.Inst.Scene;
            var ball = currentScene.AddEntity();

            currentScene.AddComponent(ball, new CBody {
                Aabb = new BoundingBox(-r * Vector3.One, r * Vector3.One),
                Radius = r,
                InvMass = 0.1f,
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
                model = Game1.Inst.Content.Load<Model>("Models/badboll"),
                fileName = "badboll"
            });
            currentScene.AddComponent(ball, new CPickUp());
            return ball;
        }

        public static Func<float, Matrix> wiggleAnimation(int id)
        {
            var randt = (float)rnd.NextDouble() * 2.0f * MathHelper.Pi;
            var currentScene = Game1.Inst.Scene;
            Func<float, Matrix> npcAnim = (t) => {
                var transf = (CTransform)currentScene.GetComponentFromEntity<CTransform>(id);
                var body = (CBody)currentScene.GetComponentFromEntity<CBody>(id);

                // Wiggle wiggle!
                var x = 0.3f * Vector3.Dot(transf.Frame.Forward, body.Velocity);
                var walk =
                    Matrix.CreateFromAxisAngle(Vector3.Forward, x * 0.1f * (float)Math.Cos(randt + t * 12.0f))
                  * Matrix.CreateTranslation(Vector3.Up * -x * 0.1f * (float)Math.Sin(randt + t * 24.0f));

                var idle = Matrix.CreateTranslation(Vector3.Up * 0.07f * (float)Math.Sin(randt + t * 2.0f));

                return walk * idle;
            };
            return npcAnim;
        }

        public static Func<float, Matrix> playerAnimation(int player,int wiggleness, float speed)
        {

            Func<float, Matrix> playerAnim = (t) => {
                var transf = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(player);
                var body = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(player);

                // Wiggle wiggle!
                var x = 0.3f * Vector3.Dot(transf.Frame.Forward, body.Velocity);
                var walk =
                    Matrix.CreateFromAxisAngle(Vector3.Forward, x * 0.1f * (float)Math.Cos(t * wiggleness))
                  * Matrix.CreateTranslation(Vector3.Up * -x * speed * (float)Math.Sin(t * wiggleness*2));

                var idle = Matrix.CreateTranslation(Vector3.Up * 0.07f * (float)Math.Sin(t * 2.0f));

                return walk * idle;
            };
            return playerAnim;
        }

        public static void CreateAnimals(int numFlocks,int worldsize) {
            var currentScene = Game1.Inst.Scene;

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
                int flockX = (int)(rnd.NextDouble() * worldsize);
                int flockZ = (int)(rnd.NextDouble() * worldsize);
                CTransform flockTransform = new CTransform { Position = new Vector3(flockX, 0, flockZ) };
                flockTransform.Position += new Vector3(-worldsize, 0, -worldsize);

                for (int i = 0; i < membersPerFlock; i++) {
                    int id = currentScene.AddEntity();
                    var npcAnim = wiggleAnimation(id);

                    if (flockAnimal.Equals("hen")) {
                        // TODO: Make animals have different animations based on state
                        CAnimation normalAnimation = new CHenNormalAnimation { animFn = npcAnim };
                        // Set a random offset to animation so not all animals are synced
                        normalAnimation.CurrentKeyframe = rnd.Next(normalAnimation.Keyframes.Count - 1);
                        // Random animation speed between 0.8-1.0
                        normalAnimation.AnimationSpeed = (float)rnd.NextDouble() * 0.2f + 0.8f;
                        currentScene.AddComponent<C3DRenderable>(id, normalAnimation);
                    } else {
                        CImportedModel modelComponent = new CImportedModel { animFn = npcAnim };
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
                    float size = 0.5f;
                    transformComponent.Scale = new Vector3(1f);
                    currentScene.AddComponent(id, transformComponent);
                    currentScene.AddComponent(id, new CBody {
                        InvMass = 0.05f,
                        Aabb = new BoundingBox(-size * new Vector3(1.0f, 2.0f, 1.0f), size * new Vector3(1.0f, 0.0f, 1.0f)),
                        LinDrag = 0.8f,
                        Velocity = Vector3.Zero,
                        Radius = 1f,
                        SpeedMultiplier = size,
                        MaxVelocity = 4,
                        Restitution = 0
                    });
                    // health value of npcs, maybe change per species/flock/member?
                    var npcHealth = 1;
                    currentScene.AddComponent(id, new CHealth { MaxHealth = npcHealth, Health = npcHealth });
                    currentScene.AddComponent(id, new CAI { Flock = flockId });
                    currentScene.AddComponent(id, new CSyncObject());

                    flock.Members.Add(id);
                }
                currentScene.AddComponent(flockId, flock);
                currentScene.AddComponent(flockId, flockTransform);
            }
        }

        public static void CreateCollectables(int numPowerUps, int worldsize) {
            var currentScene = Game1.Inst.Scene;

            // TODO: get the global value of the worldsize
            int chests = numPowerUps, hearts = numPowerUps;
            for (int i = 0; i < chests; i++) {
                var id = currentScene.AddEntity();
                currentScene.AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "chest", model = Game1.Inst.Content.Load<Model>("Models/chest") });
                var z = (float)(rnd.NextDouble() * worldsize);
                var x = (float)(rnd.NextDouble() * worldsize);
                var chestScale = 0.25f;
                currentScene.AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(chestScale) });
                currentScene.AddComponent(id, new CBody() { Aabb = new BoundingBox(-chestScale * Vector3.One, chestScale * Vector3.One) });
                currentScene.AddComponent(id, new CSyncObject());
            }
            for (int i = 0; i < hearts; i++) {
                var id = currentScene.AddEntity();
                currentScene.AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "heart", model = Game1.Inst.Content.Load<Model>("Models/heart") });
                var z = (float)(rnd.NextDouble() * worldsize);
                var x = (float)(rnd.NextDouble() * worldsize);
                currentScene.AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(1f) });
                currentScene.AddComponent(id, new CBody() { Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)) });
                currentScene.AddComponent(id, new CSyncObject());
            }
        }
        public static void SpawnBirds(WorldSceneConfig config) {
            var scene = Game1.Inst.Scene;
            var fileName = "seagull";
            var birdCount = 30;
            var halfWorld = config.HeightMapScale * 0.5f;
            for (int i = 0; i < birdCount; i++) {
                int id = scene.AddEntity();
                var randt = (float)rnd.NextDouble() * 2.0f * MathHelper.Pi;
                var rotationSpeed = (float)rnd.NextDouble() * 0.25f + 0.5f;
                var transform = new CTransform {
                    Position = new Vector3(
                        (float)rnd.NextDouble()*halfWorld-halfWorld,
                        config.WaterHeight + (float)rnd.NextDouble()*10+25,
                        (float)rnd.NextDouble()*halfWorld-halfWorld
                        ),
                    Scale = Vector3.One
                };
                Func<float, Matrix> flyingAnimation = (t) => {
                    var idle = Matrix.CreateTranslation(Vector3.Up * 0.07f * (float)Math.Sin(randt + t * 2.0f));
                    var rot = Matrix.CreateRotationY(-(t * rotationSpeed));
                    return idle * rot;
                };
                scene.AddComponent(id, transform);
                scene.AddComponent<C3DRenderable>(id, new CImportedModel {
                    fileName = fileName,
                    model = Game1.Inst.Content.Load<Model>("Models/" + fileName),
                    animFn = flyingAnimation
                });
            }
        }
		public static void SpawnEnvironment(Heightmap heightmap, int worldsize)
		{

            Func<float, float> treeFn = x => 0.2f * (float)Math.Pow(x, 2);
            Func<float, float> rockFn = x => 0.3f * (float)Math.Pow(x, 1.2);
            var elementList = new Dictionary<int, Tuple<string, float, float, Func<float, float>>>();
            // Definition for environment spawns: model name, submersion into ground, model scale, random scale function
            elementList.Add(255, new Tuple<string, float, float, Func<float, float>>("LeafTree",    0.5f,   1f,     treeFn));
            elementList.Add(245, new Tuple<string, float, float, Func<float, float>>("PalmTree",    1.0f,   1f,     treeFn));
            elementList.Add(235, new Tuple<string, float, float, Func<float, float>>("tree",        0.5f,   1.2f,   treeFn));
            elementList.Add(170, new Tuple<string, float, float, Func<float, float>>("rock2",       0.1f,   1.6f,   rockFn));

                        var matDic = new Dictionary<int, MaterialShader>();
                        matDic = null;
                        // TODO: Models are not properly vertex colored so code below makes
                        //       everything blue.
                        // var toonMat = new ToonMaterial(Vector3.One*0.2f,
                        //                                new Vector3(1.0f, 0.0f, 1.0f), // ignored
                        //                                Vector3.Zero,
                        //                                40.0f,
                        //                                null, // diftex
                        //                                null, // normtex
                        //                                1.0f, // normcoeff
                        //                                5, // diflevels
                        //                                2, // spelevels,
                        //                                true); // use vert col


                        // for (var i = 0; i < 20; i++) {
                        //     matDic[i] = toonMat;
                        // }

			for (int y = 0; y<heightmap.GetDimensions().Y; y++)
			{
				for (int x = 0; x<heightmap.GetDimensions().X; x++)
				{
					if (elementList.ContainsKey(heightmap.ColorAt(x, y).B))
					{
						int newElement = Game1.Inst.Scene.AddEntity();
                        var element = elementList[(int)heightmap.ColorAt(x, y).B];
						var wx = (x / heightmap.GetDimensions().X - 0.5f) * worldsize;
						var wy = (y / heightmap.GetDimensions().Y - 0.5f) * worldsize;
                        //Game1.Inst.Scene.AddComponent(newElement, new CBox() { Box = new BoundingBox(new Vector3(-1, -5, -1), new Vector3(1, 5, 1)), InvTransf = Matrix.Identity });
                        Func<float, Matrix> animFn = null;
                        if (element.Item1.ToLower().Contains("tree")) {
                            var axis = new Vector3((float)rnd.NextDouble()-0.5f,
                                                    0.0f,
                                                    (float)rnd.NextDouble()-0.5f);
                            axis.Normalize();

                            var p0 = MathHelper.Pi*2.0f*(float)rnd.NextDouble();
                            var w = (float)rnd.NextDouble()*0.1f+1.0f;

                            var m = Matrix.CreateFromAxisAngle(axis, 0.2f-0.4f*(float)rnd.NextDouble());

                            animFn = t => {
                                var theta = 0.03f*(float)Math.Cos(p0+t*w);
                                return m * Matrix.CreateFromAxisAngle(axis, theta);
                            };
                        }

						Game1.Inst.Scene.AddComponent(newElement, new CTransform() {
                            Position = new Vector3(
                                worldsize * (x / (float)heightmap.GetDimensions().X - 0.5f),
                                heightmap.HeightAt(wx, wy) - element.Item2,
                                worldsize * (y / (float)heightmap.GetDimensions().Y - 0.5f)),
                            Scale = Vector3.One * element.Item3 * (1+(rnd.NextDouble() > 0.5 ? 1 : -1)*(element.Item4((float)rnd.NextDouble()))),
                            Rotation = Matrix.CreateRotationY((float)rnd.NextDouble() * MathHelper.Pi * 2f)
                        });
						Game1.Inst.Scene.AddComponent<C3DRenderable>(newElement, new CImportedModel() {
                            model = Game1.Inst.Content.Load<Model>("Models/" + element.Item1),
                            fileName = element.Item1,
                            materials = matDic,
                            enableVertexColor = false,
                            animFn = animFn
                        });

					}
				}
			}
		}

        public static void CreateTriggerEvents(int numTriggers, int worldsize) {
            var currentScene = Game1.Inst.Scene;
            Random rnd = new Random();

            // TODO: get the global value of the worldsize
            for (int i = 0; i < numTriggers; i++) {
                int id = currentScene.AddEntity();
                var z = (float)(rnd.NextDouble() * worldsize);
                var x = (float)(rnd.NextDouble() * worldsize);
                currentScene.AddComponent(id, new CBody() { Radius = 5, Aabb = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), LinDrag = 0.8f });
                currentScene.AddComponent(id, new CTransform() { Position = new Vector3(x, -50, z),Scale = new Vector3(1f) });
                if (rnd.NextDouble() > 0.5) {
                    // Falling balls event
                    currentScene.OnEvent("collision", data => {
                        foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>().Keys)
                        {
                            if ((((PhysicsSystem.CollisionInfo) data).Entity1 == player &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == id)
                                ||
                                (((PhysicsSystem.CollisionInfo) data).Entity1 == id &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == player))
                            {
                                CTransform playerPosition =
                                    (CTransform) currentScene.GetComponentFromEntity<CTransform>(player);
                                for (var j = 0; j < 6; j++)
                                {
                                    var r = 0.6f + (float) rnd.NextDouble() * 2.0f;
                                    Utils.SceneUtils.CreateBall(
                                        new Vector3((float) Math.Sin(j) * j + playerPosition.Position.X,
                                                    playerPosition.Position.Y + 10f + 2.0f * j,
                                                    (float) Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                        new Vector3(0.0f, -50.0f, 0.0f), // Velocity
                                        r); // Radius
                                    //balls.Add(ballId);
                                }
                            }
                        }
                    });
                } else {
                    // Balls spawns around the player
                    currentScene.OnEvent("collision", data => {
                        foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>().Keys)
                        {
                            if ((((PhysicsSystem.CollisionInfo) data).Entity1 == player &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == id)
                                ||
                                (((PhysicsSystem.CollisionInfo) data).Entity1 == id &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == player))
                            {
                                CTransform playerPosition =
                                    (CTransform) currentScene.GetComponentFromEntity<CTransform>(player);
                                for (var j = 0; j < 6; j++)
                                {
                                    var r = 0.6f + (float) rnd.NextDouble() * 2.0f;
                                    Utils.SceneUtils.CreateBall(
                                        new Vector3((float) Math.Sin(j) * j + playerPosition.Position.X,
                                            playerPosition.Position.Y + 2f,
                                            (float) Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                        new Vector3(0.0f, 0.0f, 0.0f), // Velocity
                                            r); // Radius
                                    //balls.Add(ballId);
                                }
                            }
                        }
                    });
                }
            }
        }

        public static void CreateSplatter(float x, float z, Heightmap heightmap) {
            var graphicsDevice = Game1.Inst.GraphicsDevice;
            var scene = Game1.Inst.Scene;
            // vertical offset to avoid flickering
            var yOffset = 0.1f;

            var y = heightmap.HeightAt(x, z);

            var A = new Vector3(-0.5f, 0.0f, -0.5f);
            var B = new Vector3( 0.5f, 0.0f, -0.5f);
            var C = new Vector3( 0.5f, 0.0f,  0.5f);
            var D = new Vector3(-0.5f, 0.0f,  0.5f);
            A.Y = heightmap.HeightAt(x+A.X, z+A.Z) - y;
            B.Y = heightmap.HeightAt(x+B.X, z+B.Z) - y;
            C.Y = heightmap.HeightAt(x+C.X, z+C.Z) - y;
            D.Y = heightmap.HeightAt(x+D.X, z+D.Z) - y;

            var N = -Vector3.Cross(A - B, A - D);
            N.Normalize();

            var vertices = new VertexPositionNormalTexture[4];
            vertices[0] = new VertexPositionNormalTexture(A, N, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(B, N, new Vector2(1, 0));
            vertices[2] = new VertexPositionNormalTexture(C, N, new Vector2(1, 1));
            vertices[3] = new VertexPositionNormalTexture(D, N, new Vector2(0, 1));

            var indices = new short[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            var vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);

            var indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), indices.Length, BufferUsage.None);
            indexBuffer.SetData(indices);

            var bEffect = new BasicEffect(graphicsDevice);
            bEffect.TextureEnabled = true;
            bEffect.Texture = Game1.Inst.Content.Load<Texture2D>("Textures/splatter");

            var meshes = new List<ModelMesh>();
            var parts = new List<ModelMeshPart>();
            var bones = new List<ModelBone>();

            parts.Add(new ModelMeshPart {
                VertexBuffer = vertexBuffer,
                NumVertices = vertices.Length,
                IndexBuffer = indexBuffer,
                PrimitiveCount = indices.Length / 3
            });
            var mesh = new ModelMesh(graphicsDevice, parts);
            parts[0].Effect = bEffect;

            var bone = new ModelBone {
                Name = "Splatter",
                Transform = Matrix.Identity
            };
            bone.AddMesh(mesh);
            mesh.ParentBone = bone;

            bones.Add(bone);
            meshes.Add(mesh);

            var model = new Model(graphicsDevice, bones, meshes);

            var id = scene.AddEntity();

            scene.AddComponent<C3DRenderable>(id, new CImportedModel { model = model });
            scene.AddComponent(id, new CTransform { Position = new Vector3(x, y+yOffset, z), Scale = new Vector3((float)rnd.NextDouble()*0.5f+1.5f) });
        }
    }
}
