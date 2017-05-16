namespace EngineName.Systems {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

using EngineName.Components;
using EngineName.Core;

using Microsoft.Xna.Framework;

using static System.Math;

using static EngineName.Logging.Log;

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
        //--------------------------------------
        // PUBLIC FIELDS
        //--------------------------------------

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

        //--------------------------------------
        // PUBLIC METHODS
        //--------------------------------------

        public override int GetHashCode() {
            // Order is unimportant for collision pairs so XORing them is ok!
            return Entity1 ^ Entity2;
        }
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
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>The spatial grid used to optimize collision detection. Basically a grid of hashed
    ///          voxels.</summary>
    private Dictionary<Int64, List<KeyValuePair<int, EcsComponent>>> mSpatPart =
        new Dictionary<Int64, List<KeyValuePair<int, EcsComponent>>>();

    // Private field to avoid reallocs.
    /// <summary>Contains a list of potential body-body collisions each frame.</summary>
    private List<Pair<int, int>> mColls1 = new List<Pair<int, int>>();

    // Private field to avoid reallocs.
    /// <summary>Contains a list of potential body-box collisions each frame.</summary>
    private List<Pair<int, int>> mColls2 = new List<Pair<int, int>>();

    /// <summary>Collision detection threads.</summary>
    private Thread[] mCollThreads;

    /// <summary>The event that is set during collision detection parallelization phase.</summary>
    private readonly ManualResetEvent mCollDetStart = new ManualResetEvent(false);

    /// <summary>The event that signals that all collision detection is done.</summary>
    private readonly AutoResetEvent mCollDetEnd = new AutoResetEvent(false);

    /// <summary>The entity collision detection queue.</summary>
    private readonly Queue<KeyValuePair<int, EcsComponent>> mCollEntityQueue =
        new Queue<KeyValuePair<int, EcsComponent>>();

    /// <summary>The number of collision detection checks waiting to be performed.</summary>
    private int mNumCollChecks;

    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    /// <summary>The inverse of the spatial partitioning voxel size (in meters).</summary>
    public float InvSpatPartSize { get; set; } = 1.0f; // 1/meters along each axis

    /// <summary>Gets or sets the world bounds, as a bounding box with dimensions specified in
    ///          meters.</summary>
    public BoundingBox Bounds { get; set; } =
        new BoundingBox(-10.0f*Vector3.One, 10.0f*Vector3.One);

    /// <summary>Gets or sets the world gravity vector, in meters per seconds squraed.</summary>
    public Vector3 Gravity { get; set; } = new Vector3(0.0f, -9.81f, 0.0f);

    /// <summary>Gets or sets the map system, if a heightmap is used.</summary>
    public MapSystem MapSystem { get; set; }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the physics system.</summary>
    public override void Init() {
        base.Init();

        mCollThreads = new Thread[Environment.ProcessorCount];

        for (var i = 0; i < mCollThreads.Length; i++) {
            mCollThreads[i] = new Thread(CollDetThread);
            mCollThreads[i].Priority = ThreadPriority.BelowNormal;
            mCollThreads[i].IsBackground = true;
            mCollThreads[i].Start();
        }

        GetLog().Info($"Created {mCollThreads.Length} collision detection threads.");

#if DEBUG
        DebugOverlay.Inst.DbgStr((t, dt) => $"Coll checks: {mColls1.Count + mColls2.Count}");
        DebugOverlay.Inst.DbgStr((t, dt) => $"Coll cells: {mSpatPart.Count}");
#endif
    }

    /// <summary>Cleans up the physics system.</summary>
    public override void Cleanup() {
        base.Cleanup();

        // It's ok to just kill off the threads here.
        foreach (var thread in mCollThreads) {
            thread.Abort();
        }
    }

    /// <summary>Updates all physical bodies (<see cref="CBody"/>) and solves collisions.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Update(float t, float dt) {
        base.Update(t, dt);

        var scene = Game1.Inst.Scene;

        // Basically, use semi-implicit Euler to integrate all positions and then sweep coarsely for
        // AABB collisions. All potential collisions are passed on to the fine-phase solver.
        mColls1.Clear();
        mColls2.Clear();

            // -------------------------------------------------
            // TODO: I'm commenting this out because:
            //       1) The physics system is in the engine project - not all games want an inventory
            //          (which is a very game-specific concept) so it should not be in the engine, but
            //          rather in the game project.
            //       2) It's harmful to the performance of the physics system (which lags heavily in the
            //          Dev.Collisions scene with this check enabled).
            //
            //       My suggestion is to re-implement this functionality as a separate system in the
            //       game project.
            // Skip entities that are in an inventory, since it aint't possible to collide with them
            // while they're inside an inventory.
            //     -- Philip Arvidsson <philip@philiparvidsson.com>
            //
            //Dictionary<int, EcsComponent> inventoryComps = Game1.Inst.Scene.GetComponents<CInventory>();
            //List<int> itemsInInventory = new List<int>();
            //foreach (var inv in inventoryComps)
            //{
            //    var temp = (CInventory)inv.Value;
            //    itemsInInventory.AddRange(temp.inventory);
            //}
            // -------------------------------------------------

            // We don't want to clear sp here, just every single cell. Otherwise it will keep being
            // reallocated, and we don't want that.
            foreach (var sp in mSpatPart) {
            sp.Value.Clear();
        }

        mCollEntityQueue.Clear();
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
            CheckBodyWorld(e.Key, body, transf, aabb1);

            SpatPartInsert(e, aabb1.Min, aabb1.Max);

            mCollEntityQueue.Enqueue(e);
        }

        Volatile.Write(ref mNumCollChecks, mCollEntityQueue.Count);
        WaitHandle.SignalAndWait(mCollDetStart, mCollDetEnd);

        SolveCollisions();
    }

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Calculates the hash for a spatial partitioning voxel.</summary>
    /// <param name="x">The x-coordinate of the voxel position in 3-space (no unit!).</param>
    /// <param name="y">The y-coordinate of the voxel position in 3-space (no unit!).</param>
    /// <param name="z">The z-coordinate of the voxel position in 3-space (no unit!).</param>
    private Int64 SpatPartHash(int x, int y, int z) {
        var a = (Int64) ((ushort)(x + 32767));
        var b = (Int64)(((ushort)(y + 32767))) << 16;
        var c = (Int64)(((ushort)(z + 32767))) << 32;

        return c | b | a;
    }

    /// <summary>Inserts an entity into the spatial partitioning data structure.</summary>
    /// <param name="e">The entity to insert.</param>
    /// <param name="min">The min point of the entity bounding box.</param>
    /// <param name="max">The max point of the entity bounding box.</param>
    private void SpatPartInsert(KeyValuePair<int, EcsComponent> e, Vector3 min, Vector3 max) {
        min = InvSpatPartSize * min;
        max = InvSpatPartSize * max;

        int minX = (int)min.X;
        int minY = (int)min.Y;
        int minZ = (int)min.Z;
        int maxX = (int)max.X;
        int maxY = (int)max.Y;
        int maxZ = (int)max.Z;

        var sp = mSpatPart;
        for (var x = minX; x <= maxX; x++) {
        for (var y = minY; y <= maxY; y++) {
        for (var z = minZ; z <= maxZ; z++) {
            var hash = SpatPartHash(x, y, z);

            List<KeyValuePair<int, EcsComponent>> l;
            if (!sp.TryGetValue(hash, out l)) {
                l = new List<KeyValuePair<int, EcsComponent>>();
                sp[hash] = l;
            }

            l.Add(e);
        }}}
    }

    /// <summary>Retrieves all entities in the specified region from the spatial partioning data
    ///          structure.</summary>
    /// <param name="min">The min point of the region bounding box.</param>
    /// <param name="max">The max point of the region bounding box.</param>
    /// <param name="l">The list to store results in (to avoid reallocs).</param>
    private void SpatPartRetrieve(Vector3 min,
                                  Vector3 max,
                                  List<KeyValuePair<int, EcsComponent>> l)
    {
        min = InvSpatPartSize * min;
        max = InvSpatPartSize * max;

        int minX = (int)min.X;
        int minY = (int)min.Y;
        int minZ = (int)min.Z;
        int maxX = (int)max.X;
        int maxY = (int)max.Y;
        int maxZ = (int)max.Z;

        var sp = mSpatPart;
        for (var x = minX; x <= maxX; x++) {
        for (var y = minY; y <= maxY; y++) {
        for (var z = minZ; z <= maxZ; z++) {
            var hash = SpatPartHash(x, y, z);

            List<KeyValuePair<int, EcsComponent>> l2;
            if (sp.TryGetValue(hash, out l2)) {
                l.AddRange(l2);
            }
        }}}
    }

    /// <summary>Implements a continuous collision detection thread loop.</summary>
    private void CollDetThread() {
        // So basically, what this thread does is sleep until someone tells it to start looking for
        // potential collision.

        // Avoiding reallocs!
        var q  = new List<KeyValuePair<int, EcsComponent>>();
        var l  = new List<KeyValuePair<int, EcsComponent>>();
        var l2 = new HashSet<Pair<int, int>>();

        while (true) {
            q.Clear();

            lock (mCollEntityQueue) {
                mCollDetStart.WaitOne();

                if (mCollEntityQueue.Count == 0) {
                    mCollDetStart.Reset();
                    continue;
                }

                for (var i = 0; i < 50; i++) {
                    if (mCollEntityQueue.Count == 0) {
                        break;
                    }

                    q.Add(mCollEntityQueue.Dequeue());
                }
            }

            var scene = Game1.Inst.Scene;

            foreach (var e in q) {
                var body   = (CBody)e.Value;
                var transf = (CTransform)scene.GetComponentFromEntity<CTransform>(e.Key);
                var p1     = transf.Position;
                var aabb1  = new BoundingBox(p1 + body.Aabb.Min, p1 + body.Aabb.Max);

                // TODO: Would possibly be beneficial to do these two in parallel. Unsure.
                FindBodyBodyColls(e, aabb1, l, l2);
                FindBodyBoxColls (e, aabb1);

                if (Interlocked.Decrement(ref mNumCollChecks) == 0) {
                    mCollDetEnd.Set();
                }
            }
        }
    }

    /// <summary>Checks for and solves collisions against the world bounds.</summary>
    /// <param name="eid">The ID of the entity to check.</param>
    /// <param name="body">The body component of the entity to check.</param>
    /// <param name="transf">The transform component of the entity to check.</param>
    /// <param name="aabb">The axis-aligned bounding box component of the entity to check.</param>
    private void CheckBodyWorld(int eid, CBody body, CTransform transf, BoundingBox aabb) {
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
        if (MapSystem != null)
        {
            //sometimes nan Values??
            if (double.IsNaN(transf.Position.X))
                return;
            var mapHeight = MapSystem.HeightPosition(transf.Position.X, transf.Position.Z);

            if (aabb.Min.Y < mapHeight) {
                transf.Position.Y = mapHeight - body.Aabb.Min.Y;
                body.Velocity.Y *= -body.Restitution;

                Scene.Raise("collisionwithground", new CollisionInfo { Entity1 = eid });
            }
        }
    }

    /// <summary>Asynchronously finds all body-body collisions</summary>
    /// <param name="e">The entity to check against other bodies.</param>
    /// <param name="aabb1">The axis-aligned bounding box of the entity.</param>
    private void FindBodyBodyColls(KeyValuePair<int, EcsComponent> e,
                                   BoundingBox aabb1,
                                   List<KeyValuePair<int, EcsComponent>> l,
                                   HashSet<Pair<int, int>> colls)
    {
        var scene = Game1.Inst.Scene;

        // No collisions are solved in the loops below - we just find potential collisions and
        // store them for later (fine-phase) processing.
        //foreach (var e2 in scene.GetComponents<CBody>()) {
        colls.Clear();
        l.Clear();
        SpatPartRetrieve(aabb1.Min, aabb1.Max, l);
        var n = l.Count;
        for (var i = 0; i < n; i++) {
            var e2 = l[i];
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

            colls.Add(new Pair<int, int>(e.Key, e2.Key));
        }

        if (colls.Count > 0) {
            lock (mColls1) {
                mColls1.AddRange(colls);
            }
        }
    }

    /// <summary>Asynchronously finds all body-box collisions</summary>
    /// <param name="e">The entity to check against oriented bounding boxes.</param>
    /// <param name="aabb1">The axis-aligned bounding box of the entity.</param>
    private void FindBodyBoxColls(KeyValuePair<int, EcsComponent> e, BoundingBox aabb1) {
        var scene = Game1.Inst.Scene;
        var colls = new List<Pair<int, int>>();

        // Find collisions against boxes (oriented bounding boxes, really). Boxes are static to
        // avoid a rigid body dynamic implementation.
        foreach (var e2 in scene.GetComponents<CBox>()) {
            var box     = (CBox)e2.Value;
            var transf2 = (CTransform)scene.GetComponentFromEntity<CTransform>(e2.Key);

            // Transform into an AABB covering the entire OBB and check for collisions.
            // TODO: We could precompute this since the OBBs are static.

            var b = box.Box;
            var r = transf2.Rotation;

            // Below: Compute AABBs for OBBs.
            // TODO: This is a hack to force stackallocs. (blame this file to see previous impl.)
            var p0 = Vector3.Transform(new Vector3(b.Min.X, b.Min.Y, b.Min.Z), r);
            var p1 = Vector3.Transform(new Vector3(b.Min.X, b.Min.Y, b.Max.Z), r);
            var p2 = Vector3.Transform(new Vector3(b.Min.X, b.Max.Y, b.Min.Z), r);
            var p3 = Vector3.Transform(new Vector3(b.Min.X, b.Max.Y, b.Max.Z), r);
            var p4 = Vector3.Transform(new Vector3(b.Max.X, b.Min.Y, b.Min.Z), r);
            var p5 = Vector3.Transform(new Vector3(b.Max.X, b.Min.Y, b.Max.Z), r);
            var p6 = Vector3.Transform(new Vector3(b.Max.X, b.Max.Y, b.Min.Z), r);
            var p7 = Vector3.Transform(new Vector3(b.Max.X, b.Max.Y, b.Max.Z), r);

            var pMin = Vector3.One * Single.PositiveInfinity;
            var pMax = Vector3.One * Single.NegativeInfinity;

            pMax.X = Max(pMax.X, p0.X);
            pMax.X = Max(pMax.X, p1.X);
            pMax.X = Max(pMax.X, p2.X);
            pMax.X = Max(pMax.X, p3.X);
            pMax.X = Max(pMax.X, p4.X);
            pMax.X = Max(pMax.X, p4.X);
            pMax.X = Max(pMax.X, p5.X);
            pMax.X = Max(pMax.X, p6.X);
            pMax.X = Max(pMax.X, p7.X);
            pMax.Y = Max(pMax.Y, p0.Y);
            pMax.Y = Max(pMax.Y, p1.Y);
            pMax.Y = Max(pMax.Y, p2.Y);
            pMax.Y = Max(pMax.Y, p3.Y);
            pMax.Y = Max(pMax.Y, p4.Y);
            pMax.Y = Max(pMax.Y, p5.Y);
            pMax.Y = Max(pMax.Y, p6.Y);
            pMax.Y = Max(pMax.Y, p7.Y);
            pMax.Z = Max(pMax.Z, p0.Z);
            pMax.Z = Max(pMax.Z, p1.Z);
            pMax.Z = Max(pMax.Z, p2.Z);
            pMax.Z = Max(pMax.Z, p2.Z);
            pMax.Z = Max(pMax.Z, p3.Z);
            pMax.Z = Max(pMax.Z, p4.Z);
            pMax.Z = Max(pMax.Z, p5.Z);
            pMax.Z = Max(pMax.Z, p6.Z);
            pMax.Z = Max(pMax.Z, p7.Z);
            pMin.X = Min(pMin.X, p0.X);
            pMin.X = Min(pMin.X, p1.X);
            pMin.X = Min(pMin.X, p2.X);
            pMin.X = Min(pMin.X, p3.X);
            pMin.X = Min(pMin.X, p4.X);
            pMin.X = Min(pMin.X, p5.X);
            pMin.X = Min(pMin.X, p6.X);
            pMin.X = Min(pMin.X, p7.X);
            pMin.Y = Min(pMin.Y, p0.Y);
            pMin.Y = Min(pMin.Y, p1.Y);
            pMin.Y = Min(pMin.Y, p2.Y);
            pMin.Y = Min(pMin.Y, p3.Y);
            pMin.Y = Min(pMin.Y, p4.Y);
            pMin.Y = Min(pMin.Y, p5.Y);
            pMin.Y = Min(pMin.Y, p6.Y);
            pMin.Y = Min(pMin.Y, p7.Y);
            pMin.Z = Min(pMin.Z, p0.Z);
            pMin.Z = Min(pMin.Z, p1.Z);
            pMin.Z = Min(pMin.Z, p2.Z);
            pMin.Z = Min(pMin.Z, p3.Z);
            pMin.Z = Min(pMin.Z, p4.Z);
            pMin.Z = Min(pMin.Z, p5.Z);
            pMin.Z = Min(pMin.Z, p6.Z);
            pMin.Z = Min(pMin.Z, p7.Z);

            var aabb2 = new BoundingBox(transf2.Position + pMin, transf2.Position + pMax);

            if (!aabb1.Intersects(aabb2)) {
                // No potential collision.
                continue;
            }

            colls.Add(new Pair<int, int>(e.Key, e2.Key));
        }

        if (colls.Count > 0) {
            lock (mColls2) {
                mColls2.AddRange(colls);
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
