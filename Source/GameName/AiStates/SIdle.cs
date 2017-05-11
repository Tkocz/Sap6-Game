using EngineName;
using EngineName.Components;
using EngineName.Core;
using EngineName.Systems;
using GameName.Systems;
using EngineName.Utils;
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
        public SIdle(int id) : base(id) { }

        public override string ToString() {
            return "Idle";
        }
        public override void Handle(float t, float dt)
        {
            var aiComponent = (CAI)Game1.Inst.Scene.GetComponentFromEntity<CAI>(entityId);
            var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(entityId);
            var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(entityId);
            var flockPosition = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(aiComponent.Flock);
            var flock = (CFlock)Game1.Inst.Scene.GetComponentFromEntity<CFlock>(aiComponent.Flock);

            var rotationSpeed = 1.0f * dt;
            var movementSpeed = dt * 300f;
            
            // determine distance to flock centroid
            var centroidDistance = PositionalUtil.Distance(flock.Centroid, npcTransform.Position);
            
            // factors to enchance certain aspects of flocking behavior
            var sepFactor = 1.5f;
            var aliFactor = 1.0f;
            var cohFactor = 1.0f;

            var sep = Separation(npcTransform.Position, flock);
            sep.Normalize();
            // Alignment
            // For every nearby boid in the system, calculate the average velocity
            var ali = flock.AvgVelocity;
            ali.Normalize();
            // Cohesion
            // For the average position (i.e. center) of all nearby boids, calculate steering vector towards that position
            var coh = Vector3.Normalize(flock.Centroid - npcTransform.Position);

            sep *= sepFactor;
            ali *= aliFactor;
            coh *= cohFactor;
            if (centroidDistance > flock.Radius)
                ali = Vector3.Zero;

            var rot = sep + ali + coh;
            //var goalQuat = AISystem.GetRotation(Vector3.Forward, rot, Vector3.Up);
            //var goalRot = Quaternion.CreateFromYawPitchRoll(rot.Y, 0, 0);
            

            //var startQuat = npcTransform.Rotation.Rotation;

            //var dQuat = Quaternion.Lerp(startQuat, goalQuat, rotationSpeed);
            //npcTransform.Rotation = Matrix.CreateFromQuaternion(dQuat);
            
            // if too far from flock, steer towards flock
            if (centroidDistance > flock.Radius) {
                Vector3 dest = Vector3.Normalize(flock.Centroid - npcTransform.Position);
                var source = Vector3.Forward;
                var goalQuat = AISystem.GetRotation(source, dest, Vector3.Up);
                var startQuat = npcTransform.Rotation.Rotation;
                var dQuat = Quaternion.Lerp(startQuat, goalQuat, rotationSpeed);
                dQuat.X = 0;
                dQuat.Z = 0;
                npcTransform.Rotation = Matrix.CreateFromQuaternion(dQuat);
            }
            // else wander around
            else {
                npcTransform.Rotation *= Matrix.CreateFromYawPitchRoll(rotationSpeed, 0, 0);
            }
            npcBody.Velocity.X = (movementSpeed * npcTransform.Rotation.Forward).X;
            npcBody.Velocity.Z = (movementSpeed * npcTransform.Rotation.Forward).Z;
        }
        private Vector3 Separation(Vector3 position, CFlock flock) {
            // Separation
            // Method checks for nearby boids and steers away
            float desiredseparation = 2.5f;
            Vector3 steer = Vector3.Zero;
            int count = 0;
            // For every boid in the system, check if it's too close
            foreach(var other in flock.Members) {
                var otherTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(other);
                float d = PositionalUtil.Distance(position, otherTransform.Position);
                // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
                if ((d > 0) && (d < desiredseparation)) {
                    // Calculate vector pointing away from neighbor
                    Vector3 diff = Vector3.Subtract(position, otherTransform.Position);
                    diff.Normalize();
                    diff /= d;        // Weight by distance
                    steer += diff;
                    count++;            // Keep track of how many
                }
            }
            // Average -- divide by how many
            if (count > 0) {
                steer /= count;
            }
            /*
            // As long as the vector is greater than 0
            if (steer.X > 0 || steer.Y > 0 || steer.Z > 0) {
                // First two lines of code below could be condensed with new PVector setMag() method
                // Not using this method until Processing.js catches up
                // steer.setMag(maxspeed);

                // Implement Reynolds: Steering = Desired - Velocity
                steer.Normalize();
                steer *= (maxspeed);
                steer.sub(velocity);
                steer.limit(maxforce);
            }*/
            return steer;
        }
    }
}
