using EngineName;
using EngineName.Components;
using EngineName.Core;
using EngineName.Systems;
using EngineName.Utils;
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
    public class AISystem : EcsSystem
    {
        public Vector3 curPos;
        public AiState curState;
        public override void Init() {
            base.Init();
        }
        public override void Update(float t, float dt) {
            // TODO: Make state deciding into fuzzy logic
            foreach (var flockKeyPair in Game1.Inst.Scene.GetComponents<CFlock>()) {
                var flock = (CFlock)flockKeyPair.Value;
                //var flockTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(flockKeyPair.Key);

                // Calculate flock centroid and avg velocity
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
                    var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(npcKey);
                    var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(npcKey);
                    if (npcComponent.State == null)
                        npcComponent.State = new SIdle(npcKey);

                    // find closest enemy
                    var closestEnemyDistance = float.MaxValue;
                    var closestEnemyId = -1;
                    foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>()) {
                        if (!Game1.Inst.Scene.EntityHasComponent<CBody>(player.Key))
                            continue;
                        var playerBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(player.Key);
                        var playerTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(player.Key);

                        var distance = PositionalUtil.Distance(playerTransform.Position, npcTransform.Position);

                        if (closestEnemyDistance > distance) {
                            closestEnemyDistance = distance;
                            closestEnemyId = player.Key;
                        }
                    }
                    var enemyBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(closestEnemyId);
                    
                    // Save fuzzy values instead of recalculating on every rule
                    var closeToEnemy = CloseToEnemy(closestEnemyDistance);
                    var fastEnemySpeed = FastSpeed(enemyBody.Velocity);
                    var mediumToEnemy = MediumToEnemy(closestEnemyDistance);

                    // Test rules and set state accordingly
                    if ((closeToEnemy & fastEnemySpeed).IsTrue()) {
                        if (npcComponent.State.GetType() != typeof(SEvade))
                            npcComponent.State = new SEvade(npcKey);
                    }
                    else if (((closeToEnemy | mediumToEnemy) & !fastEnemySpeed).IsTrue()) {
                        if (npcComponent.State.GetType() != typeof(SAware))
                            npcComponent.State = new SAware(npcKey);
                    }
                    else if (npcComponent.State.GetType() != typeof(SIdle))
                        npcComponent.State = new SIdle(npcKey);

                    // Act upon state
                    npcComponent.State.Handle(t, dt);
                }
            }
        }
        // Distance to enemy
        private FuzzyNumber CloseToEnemy(float distance) {
            return FuzzyUtil.SigMF(distance, 20, 0.3);
        }
        private FuzzyNumber MediumToEnemy(float distance) {
            return (FuzzyUtil.SigMF(distance, 50, 15));
        }
        private FuzzyNumber FarToEnemy(float distance) {
            return (FuzzyUtil.GaussMF(distance, 100, -0.3));
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
            return (FuzzyUtil.SigMF(speed, 5, -2));
        }
        private FuzzyNumber MediumSpeed(Vector3 velocity) {
            double speed = Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Z, 2));
            return (FuzzyUtil.GaussMF(speed, 4, 1));
        }
        private FuzzyNumber SlowSpeed(Vector3 velocity) {
            double speed = Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Z, 2));
            return (FuzzyUtil.SigMF(speed, 2, 2));
        }
    }
}
