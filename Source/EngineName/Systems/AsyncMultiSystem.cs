namespace Engine.Systems {

//--------------------------------------
// CLASSES
//--------------------------------------

// TODO: This is not yet async!!!

/// <summary>Provides functinality for running multiple systems spread out over the CPU
             cores.</summary>
public class AsyncMultiSystem: EcsSystem {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    private readonly EcsSystem[] mSystems;

    //--------------------------------------
    // PUBLIC CONSTRUCTORS
    //--------------------------------------

    /// <summary>Initializes the async multi system.</summary>
    /// <param name="systems">The systems to run in parallel.</param>
    public AsyncMultiSystem(params EcsSystem[] systems) {
        // Defensive copy to prevent client from causing mayhem, lol.
        mSystems = (EcsSystem[])systems.Clone();
    }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Performs initialization logic specific to the system.</summary>
    public override void Init() {
        // TODO: Init doesn't have to be parallelized.
        foreach (var system in mSystems) {
            system.Init();
        }
    }

    /// <summary>Performs cleanup logic specific to the system.</summary>
    public override void Cleanup() {
        // TODO: Cleanup doesn't have to be parallelized.
        foreach (var system in mSystems) {
            system.Cleanup();
        }
    }

    /// <summary>Performs draw logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Draw(float t, float dt) {
        foreach (var system in mSystems) {
            system.Draw(t, dt);
        }
    }

    /// <summary>Performs update logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Update(float t, float dt) {
        foreach (var system in mSystems) {
            system.Update(t, dt);
        }
    }
}

}
