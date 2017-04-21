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
                new PhysicsSystem()
            );

            base.Init();

            int camera = AddEntity();
            AddComponent(camera, new CCamera());
            AddComponent(camera, new CTransform() { Position = new Vector3(0, 0, 0), Rotation = Matrix.Identity, Scale = Vector3.One });

            int id = AddEntity();
            AddComponent(id, new CTransform() { Position = new Vector3(0, 0, 15), Rotation = Matrix.Identity, Scale = Vector3.One });
            AddComponent<C3DRenderable>(id, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("rock") });

            Log.Get().Debug("TestScene initialized.");
        }
    }
}
