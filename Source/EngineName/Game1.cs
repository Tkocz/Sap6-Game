namespace EngineName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Core;
using Utils;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

// TODO: Rename this to something sane.
/// <summary>Represents the main class for game implementations.</summary>
public class Game1: Game {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The game scene stack.</summary>
    private readonly Stack<Scene> m_Scenes = new Stack<Scene>();

    /// <summary>The game class singleton instance.</summary>
    private static Game1 s_Inst;

    /*--------------------------------------
     * PUBLIC PROPERTIES
     *------------------------------------*/

    /// <summary>Gets the graphics device manager.</summary>
    public GraphicsDeviceManager Graphics { get; }

    /// <summary>Gets the game instance.</summary>
    public static Game1 Inst {
        get { return s_Inst; }
    }

    /// <summary>Gets the currently displayed game scene.</summary>
    public Scene Scene {
        get {
            // TODO: Possible race condition here, but probably unimportant.
            if (m_Scenes.Count == 0) {
                return null;
            }

            return m_Scenes.Peek();
        }
    }

    /*--------------------------------------
     * CONSTRUCTORS
     *------------------------------------*/

    /// <summary>Initializes the game singleton instance.</summary>
    /// <param name="scene">The scene to display initially.</param>
    public Game1(Scene scene) {
        Trace.Assert(AtomicUtil.CAS(ref s_Inst, null, this));

        m_Scenes.Push(scene);

        Graphics = new GraphicsDeviceManager(this);
    }

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Enters the specified scene.</summary>
    /// <param name="scene">The scene to display.</param
    public void EnterScene(Scene scene) {
        scene.Init();
        m_Scenes.Push(scene);
    }

    /// <summary>Leaves the currently displayed scene..</summary>
    public void LeaveScene() {
        if (m_Scenes.Count == 0) {
            return;
        }

        var scene = m_Scenes.Pop();
        scene.Cleanup();
    }

    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Draws the current scene.</summary>
    /// <param name="gameTime">The game time.</param>
    protected override void Draw(GameTime gameTime) {
        var scene = Scene;
        if (scene != null) {
            var t  = (float)gameTime.TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            scene.Draw(t, dt);
        }

        base.Draw(gameTime);
    }

    /// <summary>Initializes the game.</summary>
    protected override void Initialize() {
        // There is always an initial scene, so just init it here.
        Scene.Init();
    }

    /// <summary>Updates the current scene.</summary>
    /// <param name="gameTime">The game time.</param>
    protected override void Update(GameTime gameTime) {
        var scene = Scene;
        if (scene != null) {
            var t  = (float)gameTime.TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            scene.Update(t, dt);
        }

        base.Update(gameTime);
    }
}

}
