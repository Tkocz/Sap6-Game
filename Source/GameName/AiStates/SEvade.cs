using EngineName;
using EngineName.Components;
using EngineName.Core;
using EngineName.Systems;
using GameName.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.AiStates {
    public class SEvade : AiState {
        public SEvade(int id) : base(id) {}
        public override string ToString() {
            return "Evade";
        }
        public override void Handle(float t, float dt) {
            var rotationSpeed = Math.Min(1.8f * dt, 1);
            var movementSpeed = dt * 30f;
            var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(entityId);
            var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(entityId);

            var closestEnemyDistance = float.MaxValue;
            var closestEnemyId = -1;
            CTransform closestEnemyTransform = null;

            foreach (var player in Game1.Inst.Scene.GetComponents<CInput>()) {
                if (!Game1.Inst.Scene.EntityHasComponent<CBody>(player.Key))
                    continue;
                var playerTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(player.Key);

                var positionDiff = playerTransform.Position - npcTransform.Position;
                float distance = (float)Math.Sqrt(Math.Pow(positionDiff.X, 2) + 0 + Math.Pow(positionDiff.Z, 2));

                if (closestEnemyDistance > distance) {
                    closestEnemyDistance = distance;
                    closestEnemyId = player.Key;
                    closestEnemyTransform = playerTransform;
                }
            }
            if (closestEnemyId == -1)
                return;
            Vector3 dest = Vector3.Normalize(closestEnemyTransform.Position - npcTransform.Position);
            var source = Vector3.Backward;
            var goalQuat = AISystem.GetRotation(source, dest, Vector3.Up);
            var startQuat = npcTransform.Rotation.Rotation;
            var dQuat = Quaternion.Lerp(startQuat, goalQuat, rotationSpeed);
            dQuat.X = 0;
            dQuat.Z = 0;
            npcTransform.Rotation = Matrix.CreateFromQuaternion(dQuat);

            npcBody.Velocity += movementSpeed * npcTransform.Rotation.Forward;
        }
    }
}
