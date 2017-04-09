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
    private readonly float m_InvUpdateInterval;

    /// <summary>Number of draw calls since last update.</summary>
    private int m_NumDraws;

    /// <summary>Number of update calls since last update.</summary>
    private int m_NumUpdates;

    /// <summary>The original window title.</summary>
    private string m_OrigTitle;

    /// <summary>The timer used to update the title.</summary>
    private float m_Timer;

    /*--------------------------------------
     * CONSTRUCTORS
     *------------------------------------*/

    /// <summary>Initializes a new instance of the system.</summary>
    /// <param name="updatesPerSec">The number of times to update the
    ///                             information each second.</param>
    public FpsCounterSystem(int updatesPerSec) {
        m_InvUpdateInterval = 1.0f / updatesPerSec;
    }

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Retsores the original window title.</summary>
    public override void Cleanup() {
        Game1.Inst.Window.Title = m_OrigTitle;
    }

    /// <summary>Performs draw logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Draw(float t, float dt) {
        m_NumDraws++;

        m_Timer += dt;

        if (m_Timer < m_InvUpdateInterval) {
            // Nothing to do yet.
            return;
        }

        var dps = m_NumDraws / m_InvUpdateInterval;
        var ups = m_NumUpdates / m_InvUpdateInterval;
        var s   = $"(draws/s: {dps}, updates/s: {ups})";

        Game1.Inst.Window.Title = string.Format($"{m_OrigTitle} {s}");

        m_NumDraws   = 0;
        m_NumUpdates = 0;

        m_Timer -= m_InvUpdateInterval;
    }

    /// <summary>Initializes the system.</summary>
    public override void Init() {
        m_OrigTitle = Game1.Inst.Window.Title;
    }

    /// <summary>Performs update logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Update(float t, float dt) {
        m_NumUpdates++;
    }
}

}
