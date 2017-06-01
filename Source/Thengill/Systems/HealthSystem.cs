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
            Game1.Inst.Scene.OnEvent("collision", HandleDamageSword);
            Game1.Inst.Scene.OnEvent("hit", HandleDeath);
            Game1.Inst.Scene.OnEvent("death", HandleDeath);
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
            if (Vector3.Dot(collision.Normal, Vector3.Up) > Math.Cos(MathHelper.PiOver4) ||
                Vector3.Dot(-collision.Normal, Vector3.Up) > Math.Cos(MathHelper.PiOver4)) {
                var receiver = h1;
                var receiverId = e1;
                var dealer = e2;
                if (e1IsJumper) {
                    receiver = h2;
                    receiverId = e2;
                    dealer = e1;
                }
                // if entity has invincibility time left, no damage dealt
                if (receiver.InvincibilityTime > 0) return;
                receiver.Health -= receiver.DamageResistance * 1; // TODO: hard coded damage should be replaced to component-based solution
                receiver.InvincibilityTime = 1; // TODO: change hard coded time to something more appropraiate
                Game1.Inst.Scene.Raise("hit", receiverId);
                if (Game1.Inst.Scene.EntityHasComponent<CScore>(dealer))
                    ((CScore)Game1.Inst.Scene.GetComponentFromEntity<CScore>(dealer)).Score++;
            }
        }
        private void HandleDeath(object data) {
            var key = data as int?;
            if (!key.HasValue)
                return;
            // dead
            if (Game1.Inst.Scene.EntityHasComponent<CAI>(key.Value)) {
                var aiComp = (CAI)Game1.Inst.Scene.GetComponentFromEntity<CAI>(key.Value);
                var flock = (CFlock)Game1.Inst.Scene.GetComponentFromEntity<CFlock>(aiComp.Flock);
                flock.Members.Remove(key.Value);
            }
            Game1.Inst.Scene.RemoveEntity(key.Value);
        }
        private List<int> dealers = new List<int>(); 
        private void HandleDamageSword(object data)
        {
            
            var coll = data as CollisionInfo?;
            if (!coll.HasValue) return;
            var collision = coll.Value;

            var e1 = collision.Entity1;
            var e2 = collision.Entity2;


          

            CHit chit;
            CHealth h1;
            int receiverId = 0;
            if (Game1.Inst.Scene.EntityHasComponent<CHit>(e1) && Game1.Inst.Scene.EntityHasComponent<CHealth>(e2))
            {
                receiverId = e2;
                chit = (CHit)Game1.Inst.Scene.GetComponentFromEntity<CHit>(e1);
                if (chit.PlayerId == receiverId)
                    return;
                h1 = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(e2);
            }
            else if (Game1.Inst.Scene.EntityHasComponent<CHealth>(e1) && Game1.Inst.Scene.EntityHasComponent<CHit>(e2))
            {
                receiverId = e1;
                chit = (CHit)Game1.Inst.Scene.GetComponentFromEntity<CHit>(e2);
                if (chit.PlayerId == receiverId)
                    return;
                h1 = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(e1);     
            }
            else
            {
                return;
            }



            CHealth receiver = null;
            int dealer = 0;
            if(chit.IsAttacking && chit.AnimationProgress > 0.7)
            {
                receiver = h1;
                dealer = chit.PlayerId;
                chit.LastSpashed = DateTime.Now;
            }
            else
                return;

            if (receiver.InvincibilityTime > 0) return;
            receiver.Health -= receiver.DamageResistance * 1; // TODO: hard coded damage should be replaced to component-based solution
            receiver.InvincibilityTime = 1; // TODO: change hard coded time to something more appropraiate
            Game1.Inst.Scene.Raise("hit", receiverId);
            if (Game1.Inst.Scene.EntityHasComponent<CScore>(dealer))
                ((CScore)Game1.Inst.Scene.GetComponentFromEntity<CScore>(dealer)).Score++;


        }

        private float remaingTime = 0;
        private float updateInterval;

        public override void Update(float t, float dt) {
            foreach (var healthEntity in Game1.Inst.Scene.GetComponents<CHealth>()) {
                var health = (CHealth)healthEntity.Value;
                if(health.Health <= 0) {
                    Game1.Inst.Scene.Raise("death", healthEntity.Key);
                }

                // decrease invincibility time
                health.InvincibilityTime = health.InvincibilityTime > 0 ? health.InvincibilityTime - dt : 0;
            }
        }
    }
}
