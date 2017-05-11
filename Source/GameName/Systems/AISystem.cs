using EngineName;
using EngineName.Components;
using EngineName.Core;
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
        public override void Init()
        {
            base.Init();
        }
        public override void Update(float t, float dt)
        {
            // TODO: Make state deciding into fuzzy logic
            foreach (var npc in Game1.Inst.Scene.GetComponents<CAI>())
            {
                var npcComponent = (CAI)npc.Value;
                var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(npc.Key);
                var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(npc.Key);

                var closestEnemyDistance = float.MaxValue;
                var closestEnemyId = -1;

                foreach (var player in Game1.Inst.Scene.GetComponents<CInput>())
                {
                    if (!Game1.Inst.Scene.EntityHasComponent<CBody>(player.Key))
                        continue;
                    var playerBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(player.Key);
                    var playerTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(player.Key);

                    var positionDiff = playerTransform.Position - npcTransform.Position;
                    float distance = (float)Math.Sqrt(Math.Pow(positionDiff.X, 2) + Math.Pow(positionDiff.Y, 2) + Math.Pow(positionDiff.Z, 2));

                    if (closestEnemyDistance > distance)
                    {
                        closestEnemyDistance = distance;
                        closestEnemyId = player.Key;
                    }
                }
                if (npcComponent.State == null)
                    npcComponent.State = new SIdle(npc.Key);
                // Decide state
                if (closestEnemyDistance < 20) {
                    if (npcComponent.State.GetType() != typeof(SEvade))
                        npcComponent.State = new SEvade(npc.Key);
                }
                else if (closestEnemyDistance < 50) {
                    if (npcComponent.GetType() != typeof(SAware))
                        npcComponent.State = new SAware(npc.Key);
                }
                else if (npcComponent.State.GetType() != typeof(SIdle))
                    npcComponent.State = new SIdle(npc.Key);
                
                // Act upon state
                npcComponent.State.Handle(t, dt);
            }
            base.Update(t, dt);
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
