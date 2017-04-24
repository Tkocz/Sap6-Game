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
    public class PhysicsSystem: EcsSystem {
        public override void Update(float t, float dt)
        {
            foreach (CTransform transformComponent in Game1.Inst.Scene.GetComponents<CTransform>().Values)
            {
                transformComponent.Frame = Matrix.CreateScale(transformComponent.Scale) *
                                            transformComponent.Rotation *
                                            Matrix.CreateTranslation(transformComponent.Position);
            }

            foreach (var c in Game1.Inst.Scene.GetComponents<CBody>().Values) {
                var body = (CBody)c;

                // Symplectic Euler is ok for now so compute force before updating position!
                body.Position += dt*body.Velocity;
            }

            base.Update(t, dt);
        }
    }
}
