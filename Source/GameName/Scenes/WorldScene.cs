using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Logging;
using EngineName.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Scenes
{
    public class WorldScene : Scene
    {
        public override void Init()
        {
            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10), 
                new RenderingSystem(), 
                new CameraSystem(), 
                new PhysicsSystem(),
                new MapSystem()
            );
            base.Init();
            // Camera entity
            int camera = AddEntity();
            AddComponent(camera, new CCamera(){
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Game1.Inst.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f)
            });
            AddComponent(camera, new CTransform() { Position = new Vector3(0, 0, 15), Rotation = Matrix.Identity, Scale = Vector3.One });
            // Tree model entity
            int id = AddEntity();
            AddComponent<C3DRenderable>(id, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("tree") });
            AddComponent(id, new CTransform() { Position = new Vector3(0, 0, 0), Rotation = Matrix.Identity, Scale = Vector3.One });
            // Heightmap entity
            id = AddEntity();
            AddComponent<C3DRenderable>(id, new CHeightmap() { image = Game1.Inst.Content.Load<Texture2D>("HeightMap") });
            AddComponent(id, new CTransform() { Position = Vector3.Zero, Rotation = Matrix.Identity, Scale = Vector3.One });
            
            Log.Get().Debug("TestScene initialized.");
        }
    }
}
