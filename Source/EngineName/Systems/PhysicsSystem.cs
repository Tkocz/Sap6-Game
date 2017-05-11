namespace EngineName.Systems {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;

using EngineName.Components;
using EngineName.Core;

using Microsoft.Xna.Framework;

using static System.Math;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides real-time simulation of a physical world.</summary>
public class PhysicsSystem: EcsSystem {
    //--------------------------------------
    // NESTED TYPES
    //--------------------------------------

    /// <summary>Contains information about a collision.</summary>
    public struct CollisionInfo {
        /// <summary>The id of first entity involved in the collision.</summary>
        public int Entity1;
        /// <summary>The id of the second entity involved in the collision, or negative one if the
        ///           collision happened with a world bounary.</summary>
        public int Entity2;

        /// <summary>The collision force (unspecified unit).</summary>
        public float Force;

        /// <summary>The collision normal (the vector along which the collision force was applied to
        ///          resolve the collision).</summary>
        public Vector3 Normal;

        /// <summary>The point in world-space where the collision occurred.</summary>
        public Vector3 Position;
    }

    // TODO: This should be moved somewhere else. I would use the Tuple type but it's a ref type so
    //       better to create a generic pair *value* type to avoid performance issues with the
    //       garbage collector.
    /// <summary>Represents a pair of two items.</summary>
    /// <typeparam name="T1">Specifies the type of the first item in the pair.</typeparam>
    /// <typeparam name="T2">Specifies the type of the second item in the pair.</typeparam>
    private struct Pair<T1, T2> where T1: struct
                                where T2: struct
    {
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

    /// <summary>Gets or sets the world gravity vector, in meters per seconds squraed.</summary>
    public Vector3 Gravity { get; set; } = new Vector3(0.0f, -9.81f, 0.0f);

    /// <summary>Gets or sets the map system, if a heightmap is used.</summary>
    public MapSystem MapSystem { get; set; }

    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    // Private field to avoid reallocs.
    /// <summary>Contains a list of potential body-body collisions each frame.</summary>
    private List<Pair<int, int>> mColls1 = new List<Pair<int, int>>();

    // Private field to avoid reallocs.
    /// <summary>Contains a list of potential body-box collisions each frame.</summary>
    private List<Pair<int, int>> mColls2 = new List<Pair<int, int>>();

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the physics system.</summary>
    public override void Init() {
#if DEBUG
        DebugOverlay.Inst.DbgStr((t, dt) => $"Coll checks: {mColls1.Count + mColls2.Count}");
#endif
    }

    /// <summary>Updates all physical bodies (<see cref="CBody"/>) and solves collisions.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Update(float t, float dt) {
        var scene = Game1.Inst.Scene;

        // Basically, use semi-implicit Euler to integrate all positions and then sweep coarsely for
        // AABB collisions. All potential collisions are passed on to the fine-phase solver.
        mColls1.Clear();
        mColls2.Clear();

        foreach (var e in scene.GetComponents<CBody>()) {
            var body   = (CBody)e.Value;
            var transf = (CTransform)scene.GetComponentFromEntity<CTransform>(e.Key);

            // TODO: Implement 4th order Runge-Kutta for differential equations.
            // Symplectic Euler is ok for now so compute force before updating position!
            body.Velocity   += dt*(Gravity - body.InvMass*body.LinDrag*body.Velocity);
            transf.Position += dt*body.Velocity;

            if (body.EnableRot) {
                body.Rot = Quaternion.CreateFromAxisAngle(body.RotAx, body.RotVel*dt)*body.Rot;
                transf.Rotation = Matrix.CreateFromQuaternion(body.Rot);
            }

            // Setup the AABBs and see if they intersect (inner loop). Intersection means we have a
            // *potential* collision. It needs to be verified and resolved by the fine-phase solver.
            var p1    = transf.Position;
            var aabb1 = new BoundingBox(p1 + body.Aabb.Min, p1 + body.Aabb.Max);

            //----------------------------
            // Body-world collisions
            //----------------------------

            // May 7th, 2017: Refactored into own function in response to feedback from other group.
            CheckBodyWorld(body, transf, aabb1, e.Key);

            //----------------------------
            // Body-body collisions
            //----------------------------

            // No collisions are solved in the loops below - we just find potential collisions and
            // store them for later (fine-phase) processing.
            foreach (var e2 in scene.GetComponents<CBody>()) {
                // Check entity IDs (.Key) to skip double-checking each potential collision.
                if (e2.Key <= e.Key) {
                    continue;
                }

                var body2   = (CBody)e2.Value;
                var transf2 = (CTransform)scene.GetComponentFromEntity<CTransform>(e2.Key);
                var p2      = transf2.Position;
                var aabb2   = new BoundingBox(p2 + body2.Aabb.Min, p2 + body2.Aabb.Max);

                if (!aabb1.Intersects(aabb2)) {
                    // No potential collision.
                    continue;
                }

                mColls1.Add(new Pair<int, int>(e.Key, e2.Key));
            }

            //----------------------------
            // Body-box collisions
            //----------------------------

            // TODO: Shouldn't even alloc this every frame. :-(
            var p = new Vector3[8];

            // Find collisions against boxes (oriented bounding boxes, really). Boxes are static to
            // avoid a rigid body dynamic implementation.
            foreach (var e2 in scene.GetComponents<CBox>()) {
                var box     = (CBox)e2.Value;
                var transf2 = (CTransform)scene.GetComponentFromEntity<CTransform>(e2.Key);
                var p2      = transf2.Position;

                // Transform into an AABB covering the entire OBB and check for collisions.

                p[0] = new Vector3(box.Box.Min.X, box.Box.Min.Y, box.Box.Min.Z);
                p[1] = new Vector3(box.Box.Min.X, box.Box.Min.Y, box.Box.Max.Z);
                p[2] = new Vector3(box.Box.Min.X, box.Box.Max.Y, box.Box.Min.Z);
                p[3] = new Vector3(box.Box.Min.X, box.Box.Max.Y, box.Box.Max.Z);
                p[4] = new Vector3(box.Box.Max.X, box.Box.Min.Y, box.Box.Min.Z);
                p[5] = new Vector3(box.Box.Max.X, box.Box.Min.Y, box.Box.Max.Z);
                p[6] = new Vector3(box.Box.Max.X, box.Box.Max.Y, box.Box.Min.Z);
                p[7] = new Vector3(box.Box.Max.X, box.Box.Max.Y, box.Box.Max.Z);

                var pMin = Vector3.One * Single.PositiveInfinity;
                var pMax = Vector3.One * Single.NegativeInfinity;
                for (var i = 0; i < 8; i++) {
                    var q = Vector3.Transform(p[i], transf2.Rotation);
                    pMin.X = Min(pMin.X, q.X);
                    pMin.Y = Min(pMin.Y, q.Y);
                    pMin.Z = Min(pMin.Z, q.Z);
                    pMax.X = Max(pMax.X, q.X);
                    pMax.Y = Max(pMax.Y, q.Y);
                    pMax.Z = Max(pMax.Z, q.Z);
                }

                var aabb2 = new BoundingBox(transf2.Position + pMin, transf2.Position + pMax);

                if (!aabb1.Intersects(aabb2)) {
                    // No potential collision.
                    continue;
                }

                mColls2.Add(new Pair<int, int>(e.Key, e2.Key));
            }
        }

        SolveCollisions();

        base.Update(t, dt);
    }

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Checks for and solves collisions against the world bounds.</summary>
    private void CheckBodyWorld(CBody body, CTransform transf, BoundingBox aabb, int id) {
        // This function is pretty trivial, so we can solve the collision immediately - no need to
        // store it for solving during the fine-solver phase. Basically, just check the bounding box
        // against the world bounds and bounce against them with full restitution. In practice, this
        // method should only be used to prevent objects from leaving universe to prevent
        // hard-to-detect errors in the physical simulation. Don't use it for in-game mechanics.

        if (aabb.Min.X < Bounds.Min.X) {
            transf.Position.X = Bounds.Min.X - body.Aabb.Min.X;
            body.Velocity.X *= -body.Restitution;
        }
        else if (aabb.Max.X > Bounds.Max.X) {
            transf.Position.X = Bounds.Max.X - body.Aabb.Max.X;
            body.Velocity.X *= -body.Restitution;
        }

        if (aabb.Min.Y < Bounds.Min.Y) {
            transf.Position.Y = Bounds.Min.Y - body.Aabb.Min.Y;
            body.Velocity.Y *= -body.Restitution;
        }
        else if (aabb.Max.Y > Bounds.Max.Y) {
            transf.Position.Y = Bounds.Max.Y - body.Aabb.Max.Y;
            body.Velocity.Y *= -body.Restitution;
        }

        if (aabb.Min.Z < Bounds.Min.Z) {
            transf.Position.Z = Bounds.Min.Z - body.Aabb.Min.Z;
            body.Velocity.Z *= -body.Restitution;
        }
        else if (aabb.Max.Z > Bounds.Max.Z) {
            transf.Position.Z = Bounds.Max.Z - body.Aabb.Max.Z;
            body.Velocity.Z *= -body.Restitution;
        }

        // If we have a map system, check that we don't fall below the ground. This part is part of
        // game mechanics.
        if (MapSystem != null) {
            var mapHeight = MapSystem.HeightPosition(transf.Position.X, transf.Position.Z);

            if (aabb.Min.Y < mapHeight) {
                Scene.Raise("collisionwithground", new CollisionInfo {
                                                   Entity1 = id
                                                });
                    transf.Position.Y = mapHeight - body.Aabb.Min.Y;
                body.Velocity.Y *= -0.5f;
            }
        }
    }

    /// <summary>Finds and solves sphere-sphere collisions using an a posteriori approach.</summary>
    private void SolveCollisions() {
        // Iterate over the collision pairs and solve actual collisions.
        foreach (var cp in mColls1) {
            SolveBodyBody(cp);
        }

        foreach (var cp in mColls2) {
            SolveBodyBox(cp);
        }
    }

    /// <summary>Solves a potential body-body collision</summary>
    /// <param name="cp">The collision pair.</param>
    private void SolveBodyBody(Pair<int, int> cp) {
        // TODO: There's some clinging sometimes when collisions happen. Needs to be figured out.
        // Proably something to do with "Moving away from each other" check.

        var scene = Game1.Inst.Scene;

        var s1 = ((CBody)scene.GetComponentFromEntity<CBody>(cp.First));
        var s2 = ((CBody)scene.GetComponentFromEntity<CBody>(cp.Second));
        var t1 = (CTransform)scene.GetComponentFromEntity<CTransform>(cp.First);
        var t2 = (CTransform)scene.GetComponentFromEntity<CTransform>(cp.Second);

        // Any closer than this and the bodies are colliding
        var minDist = s1.Radius + s2.Radius;

        // Collision normal
        var n = t1.Position - t2.Position;

        if (n.LengthSquared() >= minDist*minDist) {
            // Not colliding.
            return;
        }

        var d = n.Length();
        n.Normalize();

        var i1 = Vector3.Dot(s1.Velocity, n);
        var i2 = Vector3.Dot(s2.Velocity, n);

        if (i1 > 0.0f && i2 < 0.0f) {
            // Moving away from each other, so don't bother with collision.
            // TODO: We could normalize n after this check, for better performance.
            return;
        }

        // TODO: There is probably some way around this double-inversion of the masses, but I'm
        //       too lazy to figure it out until it becomes a problem!
        var m1 = ((float)Abs(s1.InvMass) > 0.0001f) ? 1.0f/s1.InvMass : 0.0f;
        var m2 = ((float)Abs(s2.InvMass) > 0.0001f) ? 1.0f/s2.InvMass : 0.0f;
        var im = 1.0f/(m1 + m2);
        var p  = (2.0f*(i2 - i1))*im;

        d = (minDist - d)*im; // Mass adjusted penetration distance

        t1.Position += n*d*s1.InvMass;
        s1.Velocity += n*p*s1.InvMass*s1.Restitution;
        t2.Position -= n*d*s2.InvMass;
        s2.Velocity -= n*p*s2.InvMass*s2.Restitution;

        var c = 0.5f*(t1.Position + t2.Position);

        Scene.Raise("collision", new CollisionInfo { Entity1  = cp.First,
                                                     Entity2  = cp.Second,
                                                     Force    = p,
                                                     Normal   = n,
                                                     Position = c });
    }

    /// <summary>Solves a collision between a body and a (static) oriented bounding box.</summary>
    /// <param name="cp">The collision to solve.</param>
    private void SolveBodyBox(Pair<int, int> cp) {
        // So what's really going on here? Well, we're basically transforming the body into the
        // box's frame-of-reference and solving the collision as if the box was axis-aligned. Then,
        // we transform the normal back into world-space and apply it as an impulse to the body.
        // Seems to work nicely! :-D

        var scene = Game1.Inst.Scene;

        var body = ((CBody)scene.GetComponentFromEntity<CBody>(cp.First));
        var box  = ((CBox)scene.GetComponentFromEntity<CBox>(cp.Second));

        var bodyTransf = (CTransform)scene.GetComponentFromEntity<CTransform>(cp.First);
        var boxTransf  = (CTransform)scene.GetComponentFromEntity<CTransform>(cp.Second);

        var p = Vector3.Transform(bodyTransf.Position - boxTransf.Position, box.InvTransf);

        // Point bounded to box.
        var pc = new Vector3(Max(Min(box.Box.Max.X, p.X), box.Box.Min.X),
                             Max(Min(box.Box.Max.Y, p.Y), box.Box.Min.Y),
                             Max(Min(box.Box.Max.Z, p.Z), box.Box.Min.Z));

        var d = (p - pc);
        var minDist2 = body.Radius*body.Radius;
        var dist2 = d.LengthSquared();

        if (dist2 >= minDist2) {
            // Not colliding.
            return;
        }

        var r = body.Radius - (float)Sqrt(dist2);

        // Collision normal
        var n = Vector3.Transform(d, boxTransf.Rotation);
        if (Vector3.Dot(body.Velocity, n) > 0.0f) {
            // Body is moving away from box already.
            return;
        }

        if (n.Length() < 0.001f) {
            // TODO: Don't think this is needed?
            return;
        }

        n.Normalize();

        var e = (1.0f + body.Restitution);
        var i = Vector3.Reflect(body.Velocity, n);

        bodyTransf.Position += r*n;
        body.Velocity += e*n*Vector3.Dot(i, n);

        // Impulse along surface (tangent vector).
        var f = body.Velocity - n*Vector3.Dot(body.Velocity, n);

        body.RotVel = f.Length()/body.Radius;
        body.RotAx  = Vector3.Cross(n, f);

        body.RotAx.Normalize();
    }
}

}
