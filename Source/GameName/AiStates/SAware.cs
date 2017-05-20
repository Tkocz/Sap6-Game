using Thengill;
using Thengill.Components;
using Thengill.Core;
using Thengill.Systems;
using Thengill.Utils;
using GameName.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.AiStates
{
    /// <summary>
    /// Aware NPC state. NPC is aware of enemy so it stops moving and stares enemy down
    /// </summary>
    public class SAware : AiState
    {
        public SAware(int id) : base(id) {}
        public override string ToString() {
            return "Aware";
        }

        public override void Handle(float t, float dt) {
            var rotationSpeed = Math.Min(0.65f * dt, 1);
            var stopSpeed = 1-dt;

            var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(entityId);
            var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(entityId);

            var closestEnemyDistance = float.MaxValue;
            var closestEnemyId = -1;

            foreach (var player in Game1.Inst.Scene.GetComponents<CInput>()) {
                if (!Game1.Inst.Scene.EntityHasComponent<CBody>(player.Key))
                    continue;
                var playerTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(player.Key);

                var positionDiff = playerTransform.Position - npcTransform.Position;
                float distance = (float)Math.Sqrt(Math.Pow(positionDiff.X, 2) + 0 + Math.Pow(positionDiff.Z, 2));

                if (closestEnemyDistance > distance) {
                    closestEnemyDistance = distance;
                    closestEnemyId = player.Key;
                }
            }
            var enemyTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(closestEnemyId);

            Vector3 dest = Vector3.Normalize(enemyTransform.Position - npcTransform.Position);
            var source = Vector3.Forward;
            var goalQuat = PositionalUtil.GetRotation(source, dest, Vector3.Up);
            var startQuat = npcTransform.Rotation.Rotation;
            var dQuat = Quaternion.Lerp(startQuat, goalQuat, rotationSpeed);
            dQuat.X = 0;
            dQuat.Z = 0;
            npcTransform.Rotation = Matrix.CreateFromQuaternion(dQuat);
            npcBody.Velocity *= stopSpeed;
        }
    }
}
