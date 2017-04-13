namespace EngineName.Systems {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using Core;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Displays frames-per-second in the window title.</summary>
public sealed class FpsCounterSystem: EcsSystem {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The inverse of the update interval.</summary>
    private readonly float mInvUpdateInterval;

    /// <summary>Number of draw calls since last update.</summary>
    private int mNumDraws;

    /// <summary>Number of update calls since last update.</summary>
    private int mNumUpdates;

    /// <summary>The original window title.</summary>
    private string mOrigTitle;

    /// <summary>The timer used to update the title.</summary>
    private float mTimer;

    /*--------------------------------------
     * CONSTRUCTORS
     *------------------------------------*/

    /// <summary>Initializes a new instance of the system.</summary>
    /// <param name="updatesPerSec">The number of times to update the
    ///                             information each second.</param>
    public FpsCounterSystem(int updatesPerSec) {
        mInvUpdateInterval = 1.0f / updatesPerSec;
    }

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Retsores the original window title.</summary>
    public override void Cleanup() {
        Game1.Inst.Window.Title = mOrigTitle;
    }

    /// <summary>Performs draw logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Draw(float t, float dt) {
        mNumDraws++;

        mTimer += dt;

        if (mTimer < mInvUpdateInterval) {
            // Nothing to do yet.
            return;
        }

        var dps = mNumDraws / mInvUpdateInterval;
        var ups = mNumUpdates / mInvUpdateInterval;
        var s   = $"(draws/s: {dps}, updates/s: {ups})";

        Game1.Inst.Window.Title = string.Format($"{mOrigTitle} {s}");

        mNumDraws   = 0;
        mNumUpdates = 0;

        mTimer -= mInvUpdateInterval;
    }

    /// <summary>Initializes the system.</summary>
    public override void Init() {
        mOrigTitle = Game1.Inst.Window.Title;
    }

    /// <summary>Performs update logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Update(float t, float dt) {
        mNumUpdates++;
    }
}

}
