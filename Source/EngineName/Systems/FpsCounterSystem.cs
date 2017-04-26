namespace EngineName.Systems {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using Core;
    using EngineName.Components.Renderable;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

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
        
    /// <summary>The timer used to update the title.</summary>
    private float mTimer;

    /// <summary>FPS string composite.</summary>
    private string s;

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

        if (mTimer < mInvUpdateInterval)
        {
            // Nothing to do yet.
            return;
        }

        var dps = mNumDraws / mInvUpdateInterval;
        var ups = mNumUpdates / mInvUpdateInterval;
        s = $"(draws/s: {dps}, updates/s: {ups})";

        foreach (var component in Game1.Inst.Scene.GetComponents<C2DRenderable>())
        {
            var key = component.Key;
            if (component.Value.GetType() == typeof(CFPS))
            {
                CFPS text = (CFPS)component.Value;
                text.format = s;
            }
        }
        mNumDraws = 0;
        mNumUpdates = 0;

        mTimer -= mInvUpdateInterval;
        }

        /// <summary>Initializes the system.</summary>
        public override void Init() {
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