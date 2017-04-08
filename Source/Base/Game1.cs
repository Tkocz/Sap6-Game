namespace Sap6.Base {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Utils;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

public class Game1: Game {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    private static Game1 s_Inst;

    private readonly Stack<Scene> m_Scenes = new Stack<Scene>();

    /*--------------------------------------
     * PUBLIC PROPERTIES
     *------------------------------------*/

    public GraphicsDeviceManager Graphics { get; }

    public static Game1 Inst {
        get { return s_Inst; }
    }

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

    public Game1(Scene scene) {
        Trace.Assert(AtomicUtil.CAS(ref s_Inst, null, this));

        Graphics = new GraphicsDeviceManager(this);
    }

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    public void EnterScene(Scene scene) {
        scene.Init();
        m_Scenes.Push(scene);
    }

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

    protected override void Draw(GameTime gameTime) {
        var scene = m_Scenes.Peek();
        if (scene != null) {
            var t  = (float)gameTime.TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            scene.Draw(t, dt);
        }

        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime) {
        var scene = m_Scenes.Peek();
        if (scene != null) {
            var t  = (float)gameTime.TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            scene.Update(t, dt);
        }

        base.Update(gameTime);
    }
}

}
