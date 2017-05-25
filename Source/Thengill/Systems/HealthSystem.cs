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
      
        public DateTime lasthitTime = DateTime.Now;
        private void HandleDamage(object data)
        {
            //HandleDamageSword(data);
            var coll = data as CollisionInfo?;
            if (!coll.HasValue) return;
            var collision = coll.Value;

            var e1 = collision.Entity1;
            var e2 = collision.Entity2;


            var p1 = Game1.Inst.Scene.EntityHasComponent<CPlayer>(e1);
            var p2 = Game1.Inst.Scene.EntityHasComponent<CPlayer>(e2);

            if (p2 && p1 &&  DateTime.Now - lasthitTime > TimeSpan.FromSeconds(4))
            {
                lasthitTime = DateTime.Now;
                Game1.Inst.Scene.Raise("removeHudElement", "heart");
            }


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
                var dealer = e2;
                if (e1IsJumper) {
                    receiver = h2;
                    dealer = e1;
                }
                // if entity has invincibility time left, no damage dealt
                if (receiver.InvincibilityTime > 0) return;
                receiver.Health -= receiver.DamageResistance * 1; // TODO: hard coded damage should be replaced to component-based solution
                receiver.InvincibilityTime = 1; // TODO: change hard coded time to something more appropraiate
                if (Game1.Inst.Scene.EntityHasComponent<CScore>(dealer))
                    ((CScore)Game1.Inst.Scene.GetComponentFromEntity<CScore>(dealer)).Score++;
            }
        }
        private void HandleDamageSword(object data)
        {
            var coll = data as CollisionInfo?;
            if (!coll.HasValue) return;
            var collision = coll.Value;

            var e1 = collision.Entity1;
            var e2 = collision.Entity2;


            if (!Game1.Inst.Scene.EntityHasComponent<CHit>(e1) &&
                !Game1.Inst.Scene.EntityHasComponent<CHit>(e2))
                return;

            CHit chit;
            CHealth h1;

            if (Game1.Inst.Scene.EntityHasComponent<CHit>(e1) && Game1.Inst.Scene.EntityHasComponent<CHealth>(e2))
            {
                chit = (CHit)Game1.Inst.Scene.GetComponentFromEntity<CHit>(e1);
                h1 = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(chit.PlayerId);
            }
            else if (Game1.Inst.Scene.EntityHasComponent<CHealth>(e1))
            {
                chit = (CHit)Game1.Inst.Scene.GetComponentFromEntity<CHit>(e1);
                h1 = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(chit.PlayerId);
            }
            else
            {
                //Shoulnt happend;
                return;
            }



            CHealth receiver = null;
            int dealer = 0;
            if (chit.AnimationProgress > 0.5)
            {
                receiver = h1;
                dealer = e1;
            }
            else
                return;

            if (receiver.InvincibilityTime > 0) return;

            receiver.Health -= receiver.DamageResistance * 1; // TODO: hard coded damage should be replaced to component-based solution
            receiver.InvincibilityTime = 1; // TODO: change hard coded time to something more appropraiate
            if (Game1.Inst.Scene.EntityHasComponent<CScore>(dealer))
                ((CScore)Game1.Inst.Scene.GetComponentFromEntity<CScore>(dealer)).Score++;


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
