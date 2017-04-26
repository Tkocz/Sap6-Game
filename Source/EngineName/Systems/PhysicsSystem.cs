namespace EngineName.Systems
{

//--------------------------------------
// USINGS
//--------------------------------------

using System.Collections.Generic;

using EngineName.Components;
using EngineName.Core;

using Microsoft.Xna.Framework;

using static System.Math;

//--------------------------------------
// CLASSES
//--------------------------------------

public class PhysicsSystem: EcsSystem {
    //--------------------------------------
    // NESTED TYPES
    //--------------------------------------

    // TODO: This should be moved somewhere else. I would use the Tuple type but it's a ref type
    //       so better to create a generic pair *value* type to avoid performance issues with
    //       the garbage collector.
    /// <summary>Represents a pair of two items.</summary>
    /// <typeparam name="T1">Specifies the type of the first item in the pair.</typeparam>
    /// <typeparam name="T2">Specifies the type of the second item in the pair.</typeparam>
    private struct Pair<T1, T2> {
        //--------------------------------------
        // PUBLIC FIELDS
        //--------------------------------------

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

    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    /// <summary>Gets or sets the world bounds, as a bounding box with dimensions specified in
    ///          meters.</summary>
    public BoundingBox Bounds { get; set; } =
        new BoundingBox(-10.0f*Vector3.One, 10.0f*Vector3.One);

    /// <summary>Gets or sets the world gravity vector, in meters per seconds
    ///          squraed..</summary>
    public Vector3 Gravity { get; set; } = new Vector3(0.0f, -9.81f, 0.0f);

    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    // Private field to avoid reallocs.
    /// <summary>Contains a list of potential collisions each frame.</summary>
    private List<Pair<int, int>> mPotentialColls = new List<Pair<int, int>>();

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Updates all physical bodies (<see cref="CBody"/>) and solves collisions.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Update(float t, float dt) {
        // TODO: Why is this here? It has nothing to do with physics??
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

            // TODO: Implement 4th order Runge-Kutta for differential equations.
            // Symplectic Euler is ok for now so compute force before updating position!
            body.Velocity += dt*(Gravity - body.InvMass*body.LinDrag*body.Velocity);
            body.Position += dt*body.Velocity;

            // Setup the AABBs and see if they intersect (inner loop). Intersection means we
            // have a *potential* collision. It needs to be verified and resolved by the
            // fine-phase solver.
            var p1    = body.Position;
            var aabb1 = new BoundingBox(p1 + body.Aabb.Min, p1 + body.Aabb.Max);

            //----------------------------
            // Body-world collisions
            //----------------------------

            // TODO: Maybe refactor into own function? Looks messy.
            if (aabb1.Min.X < Bounds.Min.X) {
                body.Position.X = Bounds.Min.X - body.Aabb.Min.X;
                body.Velocity.X *= -1.0f;
            }
            else if (aabb1.Max.X > Bounds.Max.X) {
                body.Position.X = Bounds.Max.X - body.Aabb.Max.X;
                body.Velocity.X *= -1.0f;
            }

            if (aabb1.Min.Y < Bounds.Min.Y) {
                body.Position.Y = Bounds.Min.Y - body.Aabb.Min.Y;
                body.Velocity.Y *= -1.0f;
            }
            else if (aabb1.Max.Y > Bounds.Max.Y) {
                body.Position.Y = Bounds.Max.Y - body.Aabb.Max.Y;
                body.Velocity.Y *= -1.0f;
            }

            if (aabb1.Min.Z < Bounds.Min.Z) {
                body.Position.Z = Bounds.Min.Z - body.Aabb.Min.Z;
                body.Velocity.Z *= -1.0f;
            }
            else if (aabb1.Max.Z > Bounds.Max.Z) {
                body.Position.Z = Bounds.Max.Z - body.Aabb.Max.Z;
                body.Velocity.Z *= -1.0f;
            }

            // Not sure what else to do. Need to update transform to match physical body
            // position.
            ((CTransform)scene.GetComponentFromEntity<CTransform>(e.Key)).Position =
                body.Position;

            //----------------------------
            // Body-body collisions
            //----------------------------

            foreach (var e2 in scene.GetComponents<CBody>()) {
                var body2 = (CBody)e2.Value;

                // Check entity IDs (.Key) to skip double-checking each potential collision.
                if (e2.Key <= e.Key) {
                    continue;
                }

                var p2    = body2.Position;
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

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Finds and solves sphere-sphere collisions using an a posteriori
    ///          approach.</summary>
    private void SolveCollisions() {
        // TODO: There's some clinging sometimes when collisions happen. Needs to be figured
        // out. Proably something to do with "Moving away from each other" check.
        var scene = Game1.Inst.Scene;

        // Iterate over the collision pairs and solve actual collisions.
        foreach (var cp in mPotentialColls) {
            var s1 = ((CBody)scene.GetComponentFromEntity<CBody>(cp.First));
            var s2 = ((CBody)scene.GetComponentFromEntity<CBody>(cp.Second));

            // Any closer than this and the bodies are colliding
            var minDist = s1.Radius + s2.Radius;

            // Collision normal
            var n = s1.Position - s2.Position;

            if (n.LengthSquared() >= minDist*minDist) {
                // Not colliding.
                continue;
            }

            var d = n.Length();
            n.Normalize();

            var i1 = Vector3.Dot(s1.Velocity, n);
            var i2 = Vector3.Dot(s2.Velocity, n);

            if (i1 > 0.0f && i2 < 0.0f) {
                // Moving away from each other, so don't bother with collision.
                // TODO: We could normalize n after this check, for better performance.
                continue;
            }

            // TODO: Restitution is missing here.
            // TODO: There is probably some way around this double-inversion of the masses, but
            //       I'm too lazy to figure it out until it becomes a problem!
            var m1 = ((float)Abs(s1.InvMass) > 0.0001f) ? 1.0f/s1.InvMass : 0.0f;
            var m2 = ((float)Abs(s2.InvMass) > 0.0001f) ? 1.0f/s2.InvMass : 0.0f;
            var im = 1.0f/(m1 + m2);
            var p  = n*(2.0f*(i2 - i1))*im;

            d = (minDist - d)*im;

            s1.Position += n*d*s1.InvMass;
            s1.Velocity += p*s1.InvMass;

            s2.Position -= n*d*s2.InvMass;
            s2.Velocity -= p*s2.InvMass;

            // TODO: We probably want to pass the ids of the two objects colliding here.
            Scene.Raise("collision", null);
        }
    }
}

}
