using EngineName.Components;
using EngineName.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Systems
{
    public class CameraSystem : EcsSystem {
        public override void Update(float t, float dt)
        {
            foreach(var camera in Game1.Inst.Scene.GetComponents<CCamera>()) {
                CCamera cameraComponent = (CCamera)camera.Value;
                CTransform transformComponent = (CTransform) Game1.Inst.Scene.GetComponentFromEntity<CTransform>(camera.Key);

                Vector3 cameraPosition = Vector3.Subtract(transformComponent.Position, new Vector3(0, 10, 10));
                Vector3 cameraTarget = transformComponent.Position;
                cameraComponent.View = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            }
            base.Update(t, dt);
        }
    }
}
