using Thengill;
using Thengill.Components;
using Thengill.Core;
using Thengill.Systems;
using Thengill.Utils;
using GameName.AiStates;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameName.Components;

namespace GameName.Systems
{
    /// <summary>
    /// System for handling AI in the game.
    /// </summary>
    public class AISystem : EcsSystem
    {
        /// <summary>
        /// Time being scared
        /// </summary>
        public float ScaryTime = 2;
        /// <summary>
        /// Distance which is considered scary distance from a hit
        /// </summary>
        public float ScaryDistance = 10;
        public override void Init() {
            base.Init();
            Game1.Inst.Scene.OnEvent("hit", handleHit);
        }
        /// <summary>
        /// Handler for scaring animals that are close to a hit
        /// </summary>
        /// <param name="obj">ID of damage receiver</param>
        private void handleHit(object obj) {
            var key = obj as int?;
            if (!key.HasValue) return;
            var scene = Game1.Inst.Scene;
            if (!scene.EntityHasComponent<CTransform>(key.Value))
                return;
            var hitLocation = ((CTransform)scene.GetComponentFromEntity<CTransform>(key.Value)).Position;
            foreach(var aiEntity in scene.GetComponents<CAI>()) {
                if (!scene.EntityHasComponent<CTransform>(aiEntity.Key)) continue;
                var aiLocation = ((CTransform)(scene.GetComponentFromEntity<CTransform>(aiEntity.Key))).Position;
                if(PositionalUtil.Distance(hitLocation, aiLocation) <= ScaryDistance) {
                    var aiComponent = (CAI)aiEntity.Value;
                    aiComponent.State = new SEvade(aiEntity.Key);
                    aiComponent.StateLockTime = ScaryTime;
                }
            }
        }

        public override void Update(float t, float dt) {
            foreach (var flockKeyPair in Game1.Inst.Scene.GetComponents<CFlock>()) {
                var flock = (CFlock)flockKeyPair.Value;

                // Calculate flock centroid and avg velocity (used in some states)
                Vector3 theCenter = Vector3.Zero;
                Vector3 theVelocity = Vector3.Zero;
                foreach (var npcKey in flock.Members) {
                    var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(npcKey);
                    var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(npcKey);
                    theCenter = theCenter + npcTransform.Position;
                    theVelocity = theVelocity + npcBody.Velocity;
                }
                var flockSize = new Vector3(flock.Members.Count);
                flock.Centroid = theCenter / flockSize;
                flock.AvgVelocity = theVelocity / flockSize;

                foreach (var npcKey in flock.Members) {
                    var npcComponent = (CAI)Game1.Inst.Scene.GetComponentFromEntity<CAI>(npcKey);
                    var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(npcKey);
                    if (npcComponent.State == null)
                        npcComponent.State = new SIdle(npcKey);

                    if (npcComponent.StateLockTime <= 0) {
                        // find closest enemy
                        var closestEnemyDistance = float.MaxValue;
                        var closestEnemyId = -1;
                        foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>())
                        {
                            if (!Game1.Inst.Scene.EntityHasComponent<CBody>(player.Key))
                                continue;

                            var playerTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(player.Key);

                            var distance = PositionalUtil.Distance(playerTransform.Position, npcTransform.Position);

                            if (closestEnemyDistance > distance)
                            {
                                closestEnemyDistance = distance;
                                closestEnemyId = player.Key;
                            }
                        }
                        if (closestEnemyId == -1) return;
                        var enemyBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(closestEnemyId);

                        // Save fuzzy values instead of recalculating on every rule
                        var closeToEnemy = CloseToEnemy(closestEnemyDistance);
                        var fastEnemySpeed = FastSpeed(enemyBody.Velocity);
                        var mediumToEnemy = MediumToEnemy(closestEnemyDistance);

                        // Test rules and set state accordingly
                        if ((closeToEnemy & fastEnemySpeed).IsTrue())
                        {
                            if (npcComponent.State.GetType() != typeof(SEvade)) {
                                npcComponent.State = new SEvade(npcKey);
                                npcComponent.StateLockTime = ScaryTime;
                            }
                        }
                        else if ((closeToEnemy & !fastEnemySpeed).IsTrue())
                        {
                            if (npcComponent.State.GetType() != typeof(SAware))
                                npcComponent.State = new SAware(npcKey);
                        }
                        else if (npcComponent.State.GetType() != typeof(SIdle)) {
                            npcComponent.State = new SIdle(npcKey);
                        }

                    }
                    // Act upon state
                    npcComponent.State.Handle(t, dt);
                    npcComponent.StateLockTime = Math.Max(npcComponent.StateLockTime-dt, 0);
                }
            }
        }
        // Distance to enemy
        private FuzzyNumber CloseToEnemy(float distance) {
            return FuzzyUtil.SigMF(distance, 20, 0.3);
        }
        private FuzzyNumber MediumToEnemy(float distance) {
            return (FuzzyUtil.GaussMF(distance, 50, 15));
        }
        private FuzzyNumber FarToEnemy(float distance) {
            return (FuzzyUtil.SigMF(distance, 100, -0.3));
        }
        // Distance to flock
        private FuzzyNumber CloseToFlock(float distance) {
            return (FuzzyUtil.SigMF(distance, 10, 0.3));
        }
        private FuzzyNumber MediumToFlock(float distance) {
            return (FuzzyUtil.GaussMF(distance, 15, 10));
        }
        private FuzzyNumber FarToFlock(float distance) {
            return (FuzzyUtil.SigMF(distance, 20, -0.3));
        }
        // Enemy speed
        private FuzzyNumber FastSpeed(Vector3 velocity) {
            double speed = Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Z, 2));
            return (FuzzyUtil.SigMF(speed, 4.5, -2));
        }
        private FuzzyNumber MediumSpeed(Vector3 velocity) {
            double speed = Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Z, 2));
            return (FuzzyUtil.GaussMF(speed, 3, 1));
        }
        private FuzzyNumber SlowSpeed(Vector3 velocity) {
            double speed = Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Z, 2));
            return (FuzzyUtil.SigMF(speed, 1.5, 2));
        }
    }
}
