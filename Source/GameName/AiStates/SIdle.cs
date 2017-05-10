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
            var rotationSpeed = 1.0f * dt;
            var movementSpeed = dt*300f;
            var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(entityId);
            var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(entityId);
            
            npcTransform.Rotation *= Matrix.CreateFromYawPitchRoll(rotationSpeed, 0, 0);
            npcBody.Velocity = movementSpeed * npcTransform.Rotation.Forward;
        }
    }
}
