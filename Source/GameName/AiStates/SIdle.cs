using EngineName;
using EngineName.Components;
using EngineName.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.AiStates
{
    // Walk in circles
    public class SIdle : AiState
    {
        public SIdle(int id) : base(id)
        {
        }

        public override string ToString()
        {
            return "Idle";
        }
        public override void Handle(float t, float dt)
        {
            var rotationSpeed = Math.Min(0.65f * dt, 1);
            var movementSpeed = dt*15f;
            var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(entityId);
            var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(entityId);
            
            
            npcTransform.Rotation *= Matrix.CreateFromYawPitchRoll(MathHelper.PiOver4*rotationSpeed, 0, 0);
            npcBody.Velocity += movementSpeed * npcTransform.Frame.Backward;

            /*
            var enemyTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(closestEnemyId);
            Vector3 dest = Vector3.Normalize(enemyTransform.Position - npcTransform.Position);
            var source = Vector3.Left;
            var goalQuat = GetRotation(source, dest, Vector3.Up);
            var startQuat = npcTransform.Rotation.Rotation;
            var dQuat = Quaternion.Lerp(startQuat, goalQuat, rotationSpeed);
            npcTransform.Rotation = Matrix.CreateFromQuaternion(dQuat);*/
        }
    }
}
