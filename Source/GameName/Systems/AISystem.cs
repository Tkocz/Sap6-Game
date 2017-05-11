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
                    foreach (var player in Game1.Inst.Scene.GetComponents<CInput>()) {
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
                    
                    // Decide state
                    if (closestEnemyDistance < 20) {
                        if (npcComponent.State.GetType() != typeof(SEvade))
                            npcComponent.State = new SEvade(npcKey);
                    }
                    else if (closestEnemyDistance < 50) {
                        if (npcComponent.GetType() != typeof(SAware))
                            npcComponent.State = new SAware(npcKey);
                    }
                    else if (npcComponent.State.GetType() != typeof(SIdle))
                        npcComponent.State = new SIdle(npcKey);

                    // Act upon state
                    npcComponent.State.Handle(t, dt);
                }
            }
        }
        public static Quaternion GetRotation(Vector3 source, Vector3 dest, Vector3 up)
        {
            var dot = Vector3.Dot(source, dest);
            /*
            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                // vector a and b point exactly in the opposite direction, 
                // so it is a 180 degrees turn around the up-axis
                return new Quaternion(up, MathHelper.ToRadians(180.0f));
            }*/
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                // vector a and b point exactly in the same direction
                // so we return the identity quaternion
                return Quaternion.Identity;
            }
            float rotAngle = (float)Math.Acos(dot);
            var rotAxis = Vector3.Cross(source, dest);
            rotAxis.Normalize();
            return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
        }
    }
}
