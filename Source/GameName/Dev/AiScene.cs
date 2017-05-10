using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Dev
{
    public class AiScene : Scene
    {
        private RenderingSystem mRenderer;

        public override void Init()
        {
            var physicsSys = new PhysicsSystem();
            physicsSys.Bounds = new BoundingBox(
                new Vector3(-200.0f, 0, -200f), 
                200.0f * Vector3.One
            );
            //physicsSys.Gravity = Vector3.Zero;
            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10),
                new SkyBoxSystem(),
                new RenderingSystem(),
                new CameraSystem(),
                physicsSys,
                new InputSystem(),
                new Rendering2DSystem(),
                new AISystem()
            );


#if DEBUG
            AddSystem(new DebugOverlay());
#endif

            base.Init();
            
            int player = AddEntity();
            AddComponent(player, new CBody() {
                Radius = 1,
                Aabb = new BoundingBox(new Vector3(-1, 0, -1), new Vector3(1, 2, 1)),
                LinDrag = 0.8f,
                InvMass = 1,
                SpeedMultiplier = 1f,
                RotationMultiplier = 1f,
                MaxVelocity = 5
            });
            AddComponent(player, new CInput());
            AddComponent(player, new CTransform() { Position = new Vector3(100, 10, -100), Scale = new Vector3(1f) });
            AddComponent<C3DRenderable>(player, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/viking") });

            // Camera entity
            int camera = AddEntity();
            float fieldofview = MathHelper.PiOver2;
            float nearplane = 0.1f;
            float farplane = 1000f;
            AddComponent(camera, new CCamera(-10, 10)
            {
                Projection = Matrix.CreatePerspectiveFieldOfView(fieldofview, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane, farplane),
                ClipProjection = Matrix.CreatePerspectiveFieldOfView(fieldofview * 1.2f, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane * 0.5f, farplane * 1.2f)
            });
            AddComponent(camera, new CInput());
            AddComponent(camera, new CTransform() { Position = new Vector3(100, 20, -100), Rotation = Matrix.Identity, Scale = Vector3.One });
            
            SetupStaticObjects();
        }
        public override void Draw(float t, float dt)
        {
            Game1.Inst.GraphicsDevice.Clear(Color.White);
            base.Draw(t, dt);
        }

        private void SetupStaticObjects()
        {
            Random rn = new Random();
            for (int i = 0; i < 20; i++)
            {
                int id = AddEntity();
                CImportedModel modelComponent = new CImportedModel();
                double random = rn.NextDouble();
                modelComponent.fileName = "hen";
                modelComponent.model = Game1.Inst.Content.Load<Model>("Models/" + modelComponent.fileName);
                AddComponent<C3DRenderable>(id, modelComponent);

                int x = (int)(rn.NextDouble() * 200);
                int z = (int)(rn.NextDouble() * 200);
                float y = 0;
                CTransform transformComponent = new CTransform();
                transformComponent.Position = new Vector3(x, y, z);
                //transformComponent.Position += new Vector3(-590, -50, -590);
                transformComponent.Rotation = Matrix.CreateFromAxisAngle(Vector3.UnitY,
                    (float)(Math.PI * (rn.NextDouble() * 4)));
                float scale = 1;
                transformComponent.Scale = new Vector3(scale, scale, scale);
                AddComponent(id, transformComponent);
                AddComponent(id, new CBody {
                    InvMass = 1,
                    Aabb = new BoundingBox(new Vector3(0,0,0), new Vector3(1,1,1)),
                    LinDrag = 0.9f,
                    Velocity = Vector3.Zero,
                    Radius = 1f,
                    SpeedMultiplier = 0.5f,
                    MaxVelocity = 5
                });
                AddComponent(id, new CAI());
            }
        }
        private int CreateBall(Vector3 p, Vector3 v, float r = 1.0f)
        {
            var ball = AddEntity();

            AddComponent(ball, new CBody
            {
                Aabb = new BoundingBox(-r * Vector3.One, r * Vector3.One),
                Radius = r,
                LinDrag = 0.1f,
                Velocity = v
            });

            AddComponent(ball, new CTransform
            {
                Position = p,
                Rotation = Matrix.Identity,
                Scale = r * Vector3.One
            });

            AddComponent<C3DRenderable>(ball, new CImportedModel
            {
                model = Game1.Inst.Content.Load<Model>("Models/DummySphere")
            });

            return ball;
        }
        
    }
}
