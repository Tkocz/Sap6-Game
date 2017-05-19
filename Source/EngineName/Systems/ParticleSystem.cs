namespace EngineName.Systems {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;

using Components;
using Core;

//--------------------------------------
// CLASSES
//--------------------------------------

// So, why is this a separate system when we could be using PhysicsSystem? Well, we don't need the
// collision checking, only the integration of particle state. While we could theoretically merge
// CBody with some hypothetical data structure to widen the scope of the PhysicsSystem class, this
// is a more lean and modular approach, since, for the sake of performance, particles are *not*
// treated as entities in the physical world - merely as objects *acting* as if they *were* of
// physical nature, giving the wanted effect. Also, this class provides some basic functinality for
// spawning particles.

/// <summary>Provides a particle system.</summary>
public class ParticleSystem: EcsSystem {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>Particle spawn callback functions; particles waiting to be spawned.</summary>
    private readonly List<Func<EcsComponent[]>> mPartsToSpawn = new List<Func<EcsComponent[]>>();

    // Avoiding reallocs with this.
    /// <summary>Contains a list of ids of entities to remove at the end of the frame.</summary>
    private readonly List<int> mPartsToRemove = new List<int>();

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the particle system.</summary>
    public override void Init() {
        base.Init();
    }

    /// <summary>Updates the particle system state.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this method.</param>
    public override void Draw(float t, float dt) {
        // We're doing this in the draw method because we don't care about physical accuracy here,
        // we just want to render nice effects.
        base.Draw(t, dt);

        foreach (var cb in mPartsToSpawn) {
            var eid = Scene.AddEntity();

            foreach (var component in cb()) {
                Scene.AddComponent(eid, component, component.GetType());
            }
        }

        mPartsToSpawn.Clear();
        mPartsToRemove.Clear();

        foreach (var e in Scene.GetComponents<CParticle>()) {
            var part = (CParticle)e.Value;

            part.Life -= dt;
            if (part.Life <= 0.0f) {
                mPartsToRemove.Add(e.Key);
                continue;
            }

            // Symplectic Euler is definitely ok for particles.
            part.Velocity += dt*part.F();
            part.Position += dt*part.Velocity;

            // Not sure what else to do. Need to update transform to match physical part position.
            ((CTransform)Scene.GetComponentFromEntity<CTransform>(e.Key)).Position = part.Position;
        }

        foreach (var eid in mPartsToRemove) {
            Scene.RemoveEntity(eid);
        }
    }

    /// <summary>Spawns <paramref name="n"/> particles on the next frame.</summary>
    /// <param name="n">The number of particles to spawn.</param>
    /// <param name="cb">The particle spawning callback function.</param>
    public void SpawnParticles(int n, Func<EcsComponent[]> cb) {
        for (var i = 0; i < n; i++) {
            mPartsToSpawn.Add(cb);
        }
    }
}

}
