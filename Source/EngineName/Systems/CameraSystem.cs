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
                CTransform playerTransform = (CTransform) Game1.Inst.Scene.GetComponentFromEntity<CTransform>(camera.Key);
                Vector3 playerPosition = playerTransform.Position;

                var p = playerPosition;
                Vector3 c;
                var dist = cameraComponent.Distance;
                var yDist = p.Y + cameraComponent.Height;
                var h = -playerTransform.Heading;

                // Vi positionerar kamera utifrån karaktärens heading (h), p = karaktärerns position, c = kamerans position, t = kamerans target, dist = avstånd till objektet
                // yDist = höjd för kameran, samt t = p -- alltså att kamerans target är position för karaktären.
                // Då gäller c=p-[d*sin(h + pi/2), y, (-d)*cos(h + pi/2)]

                c = Vector3.Subtract(p, new Vector3((float)(dist * Math.Sin(h + Math.PI * 0.5f)), yDist, (float)((-dist) * Math.Cos(h + Math.PI * 0.5f))));

                c.Y = yDist; // Lock camera to given height

                cameraComponent.Target = p + Vector3.Up*2.2f;

                cameraComponent.View = Matrix.CreateLookAt(c, cameraComponent.Target, Vector3.Up);
            }
            base.Update(t, dt);
        }
    }
}
