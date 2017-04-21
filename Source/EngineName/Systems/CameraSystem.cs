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
        private GraphicsDevice mGraphicsDevice;

        public override void Init() {
            mGraphicsDevice = Game1.Inst.GraphicsDevice;
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
