using EngineName.Components;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Systems
{
    public class CameraSystem : EcsSystem {
        public override void Init() {
            GraphicsDevice mGraphicsDevice = Game1.Inst.GraphicsDevice;
            foreach(CCamera camera in Game1.Inst.Scene.GetComponents<CCamera>().Values) {
                camera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, mGraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
            }
            base.Init();
        }
        public override void Update(float t, float dt)
        {
            foreach(var camera in Game1.Inst.Scene.GetComponents<CCamera>()) {
                CCamera cameraComponent = (CCamera)camera.Value;
                CTransform transformComponent = (CTransform) Game1.Inst.Scene.GetComponentFromEntity<CTransform>(camera.Key);

                Vector3 cameraPosition = transformComponent.Position;
                var cameraTarget = new Vector3(0, 0, 0);

                cameraComponent.View = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            }
            base.Update(t, dt);
        }
    }
}
