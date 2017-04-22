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
            // graphic debugging
            RasterizerState s = new RasterizerState();
            s.CullMode = CullMode.CullClockwiseFace;
            //s.FillMode = FillMode.WireFrame;
            //Game1.Inst.GraphicsDevice.RasterizerState = s;

            var mapSystem = new MapSystem();
            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10), 
                new RenderingSystem(), 
                new CameraSystem(), 
                new PhysicsSystem(),
                mapSystem
            );
            base.Init();
            // Camera entity
            int camera = AddEntity();
            AddComponent(camera, new CCamera(){
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f)
            });
            AddComponent(camera, new CTransform() { Position = new Vector3(0, 100, 100), Rotation = Matrix.Identity, Scale = Vector3.One });
            // Tree model entity
            int id = AddEntity();
            AddComponent<C3DRenderable>(id, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("tree") });
            AddComponent(id, new CTransform() { Position = new Vector3(0, 0, 0), Rotation = Matrix.Identity, Scale = Vector3.One });
            // Heightmap entity
            id = AddEntity();
            AddComponent<C3DRenderable>(id, new CHeightmap() { image = Game1.Inst.Content.Load<Texture2D>("HeightMap") });
            AddComponent(id, new CTransform() { Position = new Vector3(-590, -50, -590), Rotation = Matrix.Identity, Scale = Vector3.One });
            // manually start loading all heightmap components, should be moved/automated
            mapSystem.Load();

            Log.Get().Debug("TestScene initialized.");
        }
    }
}
