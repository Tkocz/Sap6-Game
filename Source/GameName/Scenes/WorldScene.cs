using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Logging;
using EngineName.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameName.Scenes
{
    public class WorldScene : Scene
    {
        public override void Init() {

            var mapSystem = new MapSystem();
            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10),
                new RenderingSystem(),
                new CameraSystem(),
                new PhysicsSystem(),
                new InputSystem(),
                new Rendering2DSystem(),
            mapSystem
            );
            base.Init();
            // Camera entity
            int camera = AddEntity();
            float fieldofview = MathHelper.PiOver2;
            float nearplane = 0.1f;
            float farplane = 1000f;

            int player = AddEntity();
            AddComponent(player, new CBody() { Radius = 1, Aabb = new BoundingBox(-1 * Vector3.One, 1 * Vector3.One) } );
            AddComponent(player, new CInput());
            AddComponent(player, new CTransform() { Scale = new Vector3(1) } );
            AddComponent<C3DRenderable>(player, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/DummySphere") });

            int ball = AddEntity();
            AddComponent(ball, new CBody() { Position = new Vector3(10f, 0, 10f), Radius = 1, Aabb = new BoundingBox(-1 * Vector3.One, 1 * Vector3.One) } );
            AddComponent(ball, new CTransform() { Scale = new Vector3(1) } );
            AddComponent<C3DRenderable>(ball, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/DummySphere") });


            AddComponent(camera, new CCamera(-5, 5){
                Projection = Matrix.CreatePerspectiveFieldOfView(fieldofview, Game1.Inst.GraphicsDevice.Viewport.AspectRatio,nearplane,farplane)
                ,ClipProjection = Matrix.CreatePerspectiveFieldOfView(fieldofview*1.2f, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, nearplane*0.5f, farplane*1.2f)
            });
            AddComponent(camera, new CInput());
            AddComponent(camera, new CTransform() { Position = new Vector3(-5, 5, 0), Rotation = Matrix.Identity, Scale = Vector3.One });

            int eid = AddEntity();
            AddComponent<C2DRenderable>(eid, new CText
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034"),
                format = "Sap my Low-Poly Game",
                color = Color.White,
                position = new Vector2(300, 20)
            });
            eid = AddEntity();
            AddComponent<C2DRenderable>(eid, new CSprite
            {
                texture = Game1.Inst.Content.Load<Texture2D>("Textures/clubbing"),
                position = new Vector2(300, 300),
                color = Color.White
            });

            // Tree model entity
            /*int id = AddEntity();
            AddComponent<C3DRenderable>(id, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/tree") });
            AddComponent(id, new CTransform() { Position = new Vector3(0, 0, 0), Rotation = Matrix.Identity, Scale = Vector3.One });*/


            // Heightmap entity
            int id = AddEntity();
            AddComponent<C3DRenderable>(id, new CHeightmap() { Image = Game1.Inst.Content.Load<Texture2D>("Textures/HeightMap") });
            AddComponent(id, new CTransform() { Position = new Vector3(-590, -900, -590) * 0.01f, Rotation = Matrix.Identity, Scale = new Vector3(0.01f) });
            // manually start loading all heightmap components, should be moved/automated
            mapSystem.Load();

            Log.Get().Debug("TestScene initialized.");
        }
    }
}
