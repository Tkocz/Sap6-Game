using Thengill.Components;
using Thengill.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Thengill.Systems.PhysicsSystem;

namespace Thengill.Systems {
    public class HealthSystem : EcsSystem {
        public override void Init() {
            // Create event hooks for damage
            Game1.Inst.Scene.OnEvent("collision", HandleDamage);
        }
        private void HandleDamage(object data) {
            var coll = data as CollisionInfo?;
            if (!coll.HasValue) return;
            var collision = coll.Value;

            var e1 = collision.Entity1;
            var e2 = collision.Entity2;

            if (!Game1.Inst.Scene.EntityHasComponent<CInput>(e1) &&
                !Game1.Inst.Scene.EntityHasComponent<CInput>(e2))
                return;

            if (!Game1.Inst.Scene.EntityHasComponent<CHealth>(e1) ||
                !Game1.Inst.Scene.EntityHasComponent<CHealth>(e2))
                return;

            var h1 = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(e1);
            var h2 = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(e2);
            var e1IsJumper = Game1.Inst.Scene.EntityHasComponent<CInput>(e1);
            // Check for damage collision (testing for jumping on something)
            if (Vector3.Dot(collision.Normal, Vector3.Up) > Math.Cos(MathHelper.PiOver4)) {
                var receiver = h1;
                if (e1IsJumper)
                    receiver = h2;
                // if entity has invincibility time left, no damage dealt
                if (receiver.InvincibilityTime > 0) return;
                receiver.Health -= receiver.DamageResistance * 1; // TODO: hard coded damage should be replaced to component-based solution
                receiver.InvincibilityTime = 1; // TODO: change hard coded time to something more appropraiate
            }
        }
        public override void Update(float t, float dt) {
            foreach(var healthEntity in Game1.Inst.Scene.GetComponents<CHealth>()) {
                var health = (CHealth)healthEntity.Value;
                if(health.Health <= 0) {
                    // dead
                    if(Game1.Inst.Scene.EntityHasComponent<CAI>(healthEntity.Key)) {
                        var aiComp = (CAI)Game1.Inst.Scene.GetComponentFromEntity<CAI>(healthEntity.Key);
                        var flock = (CFlock)Game1.Inst.Scene.GetComponentFromEntity<CFlock>(aiComp.Flock);
                        flock.Members.Remove(healthEntity.Key);
                    }
                    Game1.Inst.Scene.RemoveEntity(healthEntity.Key);
                }
                // decrease invincibility time
                health.InvincibilityTime = health.InvincibilityTime > 0 ? health.InvincibilityTime - dt : 0;
            }
        }
    }
}
