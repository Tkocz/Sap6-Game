using Thengill;
using Thengill.Components;
using Thengill.Core;

namespace GameName.Systems {
    public class HitSystem : EcsSystem {
        private const float MAXROT = 0.3f;
        public override void Init() {
            /*Game1.Inst.Scene.OnEvent("attack", data => {
                var p = (CPlayer)Game1.Inst.Scene.GetComponentFromEntity<CPlayer>((int)data);
                p.IsAttacking = true;

            });*/
            base.Init();
        }
        public override void Update(float t, float dt) {

            foreach (var attack in Game1.Inst.Scene.GetComponents<CPlayer>()) {
                var attackData = (CPlayer)attack.Value;
                /*
                if(attackData.IsAttacking) {
                    attackData.Angle += dt * attackData.AngleDir;
                    if (attackData.Angle > MAXROT) {
                        attackData.Angle = MAXROT;
                        attackData.AngleDir = -1;
                    } else if (attackData.Angle < -MAXROT) {
                        attackData.AngleDir = 1;
                        attackData.Angle = -MAXROT;
                    }
                }*/

            }

            base.Update(t, dt);
        }
    }
}
