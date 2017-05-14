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
    public class SIdle : AiState {
        Random rnd = new Random();
        public SIdle(int id) : base(id) { }

        public override string ToString() {
            return "Idle";
        }
        public override void Handle(float t, float dt)
        {
            var aiComponent = (CAI)Game1.Inst.Scene.GetComponentFromEntity<CAI>(entityId);
            var npcTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(entityId);
            var npcBody = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(entityId);
            var flock = (CFlock)Game1.Inst.Scene.GetComponentFromEntity<CFlock>(aiComponent.Flock);
            var flockTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(aiComponent.Flock);

            var rotationSpeed = 1.0f * dt;
            var movementSpeed = dt * flock.PreferredMovementSpeed;
            
            var separation = Separation(npcTransform.Position, flock);
            var alignment = flock.AvgVelocity/flock.Members.Count;
            var cohesion = flock.Centroid - npcTransform.Position;
            // try to make flock stay at the same position
            cohesion = flockTransform.Position - npcTransform.Position;
            cohesion.Y = 0;
            // factors to enchance certain aspects of flocking behavior
            separation *= flock.SeparationFactor;
            alignment *= flock.AlignmentFactor;
            cohesion *= flock.CohesionFactor;
            
            // add all directions and then normalize to get a goal direction
            var rot = separation+cohesion+alignment;
            rot.Normalize();
            var source = Vector3.Forward;
            var goalQuat = PositionalUtil.GetRotation(source, rot, Vector3.Up);
            // add a little randomness to goal direction
            var randomDirection = (float)rnd.NextDouble() * 0.25f;
            goalQuat.Y += randomDirection;

            var startQuat = npcTransform.Rotation.Rotation;
            var dQuat = Quaternion.Lerp(startQuat, goalQuat, rotationSpeed);
            dQuat.Normalize();
            dQuat.X = 0;
            dQuat.Z = 0;
            // set rotation to interpolated direction
            npcTransform.Rotation = Matrix.CreateFromQuaternion(dQuat);

            // move forward in set direction
            npcBody.Velocity.X = (movementSpeed * npcTransform.Rotation.Forward).X;
            npcBody.Velocity.Z = (movementSpeed * npcTransform.Rotation.Forward).Z;
        }
        private Vector3 Separation(Vector3 position, CFlock flock) {
            // Separation
            // Method checks for nearby boids and steers away
            Vector3 steer = Vector3.Zero;
            int count = 0;
            // For every boid in the system, check if it's too close
            foreach(var other in flock.Members) {
                var otherTransform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(other);
                float d = PositionalUtil.Distance(position, otherTransform.Position);
                // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
                if ((d > 0) && (d < flock.SeparationDistance)) {
                    // Calculate vector pointing away from neighbor
                    Vector3 diff = Vector3.Normalize(position - otherTransform.Position);
                    
                    //Vector3 diff = Vector3.Subtract(position, otherTransform.Position);
                    //diff.Normalize();
                    diff /= d;        // Weight by distance (makes closer neighbors more important to steer away from)
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
            //steer.Normalize();

            return steer;
        }
    }
}
