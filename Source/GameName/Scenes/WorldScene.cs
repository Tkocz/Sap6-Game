using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Logging;
using EngineName.Systems;
using EngineName.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameName.Scenes
{
    public class WorldScene : Scene
    {
        private float passedTime = 0.0f;
        private bool shouldLeave = false;
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
            physicsSys.Bounds = new BoundingBox(-200.0f*Vector3.One, 200.0f*Vector3.One);
            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10),
                new SkyBoxSystem(),
                new RenderingSystem(),
                new CameraSystem(),
                physicsSys,
                mapSystem,
                new InputSystem(mapSystem),
                waterSys,
                new Rendering2DSystem()

            );

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

            base.Init();
            // Camera entity
            int camera = AddEntity();
            float fieldofview = MathHelper.PiOver2;
            float nearplane = 0.1f;
            float farplane = 1000f;

            int player = AddEntity();
            AddComponent(player, new CBody() { SpeedMultiplier = 100f, Radius = 1, Aabb = new BoundingBox(new Vector3(-1, -2, -1), new Vector3(1, 2, 1)), LinDrag = 0.8f } );
            AddComponent(player, new CInput());
            AddComponent(player, new CTransform() { Position = new Vector3(0, -0, 0), Scale = new Vector3(0.01f) } );
            AddComponent<C3DRenderable>(player, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/viking") });
            /*
            int ball = AddEntity();
            AddComponent(ball, new CBody() { Position = new Vector3(10f, 0, 10f), Radius = 1, Aabb = new BoundingBox(-1 * Vector3.One, 1 * Vector3.One) } );
            AddComponent(ball, new CTransform() { Scale = new Vector3(1) } );
            AddComponent<C3DRenderable>(ball, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/DummySphere") });
            */

            AddComponent(camera, new CCamera(-50, 50) {
                Projection = Matrix.CreatePerspectiveFieldOfView(fieldofview, Game1.Inst.GraphicsDevice.Viewport.AspectRatio,nearplane,farplane)
                ,ClipProjection = Matrix.CreatePerspectiveFieldOfView(fieldofview*1.2f, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane*0.5f, farplane*1.2f)
            });
            AddComponent(camera, new CInput());
            AddComponent(camera, new CTransform() { Position = new Vector3(-50, 50, 0), Rotation = Matrix.Identity, Scale = Vector3.One });
            /*
            int eid = AddEntity();
            AddComponent<C2DRenderable>(eid, new CFPS
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034"),
                format = "Sap my Low-Poly Game",
                color = Color.White,
                position = new Vector2(300, 20),
                origin = Vector2.Zero// Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034").MeasureString("Sap my Low-Poly Game") / 2
        });
            eid = AddEntity();
            AddComponent<C2DRenderable>(eid, new CSprite
            {
                texture = Game1.Inst.Content.Load<Texture2D>("Textures/clubbing"),
                position = new Vector2(300, 300),
                color = Color.White
            });
            */
            // Tree model entity
            /*int id = AddEntity();
            AddComponent<C3DRenderable>(id, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/tree") });
            AddComponent(id, new CTransform() { Position = new Vector3(0, 0, 0), Rotation = Matrix.Identity, Scale = Vector3.One });*/


            // Heightmap entity
            int id = AddEntity();
            AddComponent<C3DRenderable>(id, new CHeightmap() { Image = Game1.Inst.Content.Load<Texture2D>("Textures/HeightMap") });
            AddComponent(id, new CTransform() { Position = new Vector3(-590, -50, -590), Rotation = Matrix.Identity, Scale = new Vector3(1) });
            // manually start loading all heightmap components, should be moved/automated
            mapSystem.Load();
            waterSys.Load();
            physicsSys.MapSystem = mapSystem;

            var rnd = new Random();
            for (var i = 0; i < 200; i++) {
                var r = 0.6f + (float)rnd.NextDouble() * 1.0f;
                CreateBall(new Vector3(-4.5f + i * 0.05f + 100, 20.0f + 2.0f * i, (float)Math.Cos(i) + 100), // Position
                           new Vector3(0.0f, 0.0f, 0.0f), // Velocity
                           r);                                                               // Radius
            }

            // Add tree as sprint goal

            int sprintGoal = AddEntity();
            AddComponent(sprintGoal, new CBody() { Radius = 1, Aabb = new BoundingBox(new Vector3(-1, -2, -1), new Vector3(1, 2, 1)), LinDrag = 0.8f });
            AddComponent(sprintGoal, new CTransform() { Position = new Vector3(100, -0, 100), Scale = new Vector3(0.05f) });
            AddComponent<C3DRenderable>(sprintGoal, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/tree") });


            OnEvent("collision", data => {
                if ((((PhysicsSystem.CollisionInfo)data).Entity1 == player &&
                     ((PhysicsSystem.CollisionInfo)data).Entity2 == sprintGoal)
                    || 
                    (((PhysicsSystem.CollisionInfo)data).Entity2 == player &&
                     ((PhysicsSystem.CollisionInfo)data).Entity1 == sprintGoal)) {
                    shouldLeave = true;
                                       
                }
            });


            Log.Get().Debug("TestScene initialized.");
        }

        public override void Update(float t, float dt) {
            passedTime += dt;
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

            AddComponent<C3DRenderable>(ball, new CImportedModel {
                model = Game1.Inst.Content.Load<Model>("Models/DummySphere")
            });

            return ball;
        }
    }
}
