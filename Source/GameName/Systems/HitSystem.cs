using Microsoft.Xna.Framework;
using System;
using Thengill;
using Thengill.Components;
using Thengill.Core;

namespace GameName.Systems {
    public class HitSystem : EcsSystem {
        private bool forwardAnimation = true;
        public struct HitInfo {
            public int EntityID;
            public float StartTime;
            public bool IsAttacking;
        }
        public override void Init() {
            Game1.Inst.Scene.OnEvent("attack", data => {
                HitInfo info = (HitInfo)data;
                var cplay = (CPlayer)Game1.Inst.Scene.GetComponentFromEntity<CPlayer>(info.EntityID);
                var chit = (CHit)Game1.Inst.Scene.GetComponentFromEntity<CHit>(cplay.HitId);

                chit.IsAttacking = info.IsAttacking;
                chit.StartTime   = info.StartTime;

            });
            base.Init();
        }
        public override void Update(float t, float dt) {

            foreach (var p in Game1.Inst.Scene.GetComponents<CHit>()) {
                var attackData = (CHit)p.Value;
                if (attackData.IsAttacking) {
                    var progress = (t - attackData.StartTime) / attackData.AnimationTime;
                    float radians;

                    if (forwardAnimation)
                        radians = MathHelper.Lerp(0, MathHelper.Pi, Math.Min(progress, 1));
                    else
                        radians = MathHelper.Lerp(MathHelper.Pi, 0, Math.Max(progress, 0));

                    attackData.AnimationProgress = radians;

                    if (forwardAnimation && radians > 1f) {
                        forwardAnimation = false;
                        attackData.AnimationProgress = 1;
                    }
                    if (!forwardAnimation && radians < 0) {
                        forwardAnimation = true;
                        attackData.AnimationProgress = 0;
                        attackData.IsAttacking = false;
                    }
                }
                
                var chittransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(p.Key);
                var cplayertransform = (CTransform) Game1.Inst.Scene.GetComponentFromEntity<CTransform>(attackData.PlayerId);
                var playerRot = Matrix.CreateRotationY(cplayertransform.Heading) * Matrix.CreateTranslation(cplayertransform.Position);
                chittransform.Position = playerRot.Translation + attackData.HitBoxOffset;
               
            }

            base.Update(t, dt);
        }
    }
}
