using EngineName.Components;
using EngineName.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Systems
{
    using static System.Math;

    public class PhysicsSystem: EcsSystem {
        private sealed class CollInfo {
            public int E1;
            public int E2;
        }

        // Private field to avoid reallocs.
        /// <summary>Contains a list of potential collisions each frame.</summary>
        private List<CollInfo> mPotentialColls = new List<CollInfo>();

        public override void Update(float t, float dt)
        {
            foreach (CTransform transformComponent in Game1.Inst.Scene.GetComponents<CTransform>().Values)
            {
                transformComponent.Frame = Matrix.CreateScale(transformComponent.Scale) *
                                            transformComponent.Rotation *
                                            Matrix.CreateTranslation(transformComponent.Position);
            }

            // Basically, use semi-implicit Euler to integrate all positions and then sweep coarsely
            // for AABB collisions. All potential collisions are passed on to the fine-phase solver.
            mPotentialColls.Clear();
            foreach (var e in Game1.Inst.Scene.GetComponents<CBody>()) {
                var body = (CBody)e.Value;

                // Symplectic Euler is ok for now so compute force before updating position!
                body.Position += dt*body.Velocity;

                // Not sure what else to do. Need to update transform to match physical body
                // position.
                ((CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(e.Key)).Position =
                    body.Position;

                foreach (var e2 in Game1.Inst.Scene.GetComponents<CBody>()) {
                    var body2 = (CBody)e2.Value;

                    // Check entity IDs (.Key) to skip double-checking each potential collision.
                    if (e2.Key <= e.Key) {
                        continue;
                    }

                    // Seutp the two AABBs and see if they intersect. Intersection means we have
                    // a *potential* collision. It needs to be verified by the fine-phase solver.
                    var p1    = body .Position;
                    var p2    = body2.Position;
                    var aabb1 = new BoundingBox(p1 + body .Aabb.Min, p1 + body .Aabb.Max);
                    var aabb2 = new BoundingBox(p2 + body2.Aabb.Min, p2 + body2.Aabb.Max);

                    if (!aabb1.Intersects(aabb2)) {
                        // No potential collision.
                        continue;
                    }

                    mPotentialColls.Add(new CollInfo { E1 = e.Key, E2 = e2.Key });
                }
            }

            //if (mPotentialColls.Count > 0) {
            //    Log.Get().Debug($"Found {mPotentialColls.Count} potential collisions.");
            //}

            SolveCollisions();

            base.Update(t, dt);
        }

        /// <summary>Finds and solves sphere-sphere collisions using an a posteriori
        ///          approach.</summary>
        private void SolveCollisions() {
            foreach (var ci in mPotentialColls) {
                var s1 = ((CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(ci.E1));
                var s2 = ((CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(ci.E2));

                var minDist = 1.0f + 1.0f;

                var n = s1.Position - s2.Position;

                if (n.LengthSquared() >= minDist*minDist) {
                    // Not colliding.
                    continue;
                }

                n.Normalize();

                var i1 = Vector3.Dot(s1.Velocity, n);
                var i2 = Vector3.Dot(s2.Velocity, n);

                if (i1 > 0.0f && i2 < 0.0f) {
                    // Moving away from each other, so don't bother with collision.
                    continue;
                }

                var p = n* (2.0f * (i1 - i2)) * 0.5f; // * 0.5 = / sums of masses

                s1.Velocity -= p*s1.InvMass;
                s2.Velocity += p*s2.InvMass;
            }
        }
    }
}
