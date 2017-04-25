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
        // TODO: This should be moved somewhere else. I would use the Tuple type but it's a ref type
        //       so better to create a generic pair *value* type to avoid performance issues with
        //       the garbage collector.
        /// <summary>Represents a pair of two items.</summary>
        /// <typeparam name="T1">Specifies the type of the first item in the pair.</typeparam>
        /// <typeparam name="T2">Specifies the type of the second item in the pair.</typeparam>
        private struct Pair<T1, T2> {
            /// <summary>The first item in the pair.</summary>
            public T1 First;

            /// <summary>The second item in the pair.</summary>
            public T2 Second;

            /// <summary>Initializes a new pair.</summary>
            /// <param name="first">The first item in the pair.</param>
            /// <param name="second">The second item in the pair.</param>
            public Pair(T1 first, T2 second) {
                First  = first;
                Second = second;
            }
        }

        // Private field to avoid reallocs.
        /// <summary>Contains a list of potential collisions each frame.</summary>
        private List<Pair<int, int>> mPotentialColls = new List<Pair<int, int>>();

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
            var scene = Game1.Inst.Scene;
            foreach (var e in scene.GetComponents<CBody>()) {
                var body = (CBody)e.Value;

                // Symplectic Euler is ok for now so compute force before updating position!
                body.Position += dt*body.Velocity;

                // Not sure what else to do. Need to update transform to match physical body
                // position.
                ((CTransform)scene.GetComponentFromEntity<CTransform>(e.Key)).Position =
                    body.Position;

                foreach (var e2 in scene.GetComponents<CBody>()) {
                    var body2 = (CBody)e2.Value;

                    // Check entity IDs (.Key) to skip double-checking each potential collision.
                    if (e2.Key <= e.Key) {
                        continue;
                    }

                    // Setup the two AABBs and see if they intersect. Intersection means we have a
                    // *potential* collision. It needs to be verified and resolved by the fine-phase
                    // solver.
                    var p1    = body .Position;
                    var p2    = body2.Position;
                    var aabb1 = new BoundingBox(p1 + body .Aabb.Min, p1 + body .Aabb.Max);
                    var aabb2 = new BoundingBox(p2 + body2.Aabb.Min, p2 + body2.Aabb.Max);

                    if (!aabb1.Intersects(aabb2)) {
                        // No potential collision.
                        continue;
                    }

                    mPotentialColls.Add(new Pair<int, int>(e.Key, e2.Key));
                }
            }

            SolveCollisions();

            base.Update(t, dt);
        }

        /// <summary>Finds and solves sphere-sphere collisions using an a posteriori
        ///          approach.</summary>
        private void SolveCollisions() {
            var scene = Game1.Inst.Scene;

            // Iterate over the collision pairs and solve actual collisions.
            foreach (var cp in mPotentialColls) {
                var s1 = ((CBody)scene.GetComponentFromEntity<CBody>(cp.First));
                var s2 = ((CBody)scene.GetComponentFromEntity<CBody>(cp.Second));

                // TODO: Simple sum of radii, need to be able to set this on each sphere.
                var minDist = 1.0f + 1.0f;

                // Collision normal
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
                    // TODO: We could normalize n after this check.
                    continue;
                }

                var p = n*(2.0f*(i1 - i2)) * 0.5f; // * 0.5 = / sums of masses, so leave it here

                s1.Velocity -= p*s1.InvMass;
                s2.Velocity += p*s2.InvMass;
            }
        }
    }
}
