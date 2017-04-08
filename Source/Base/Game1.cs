namespace Sap6.Base {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

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

    /*--------------------------------------
     * PUBLIC PROPERTIES
     *------------------------------------*/

    public GraphicsDeviceManager Graphics { get; }

    public static Game1 Inst {
        get { return s_Inst; }
    }

    public Scene Scene { get; private set; }

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
        scene.Parent = Scene;
        Scene = scene;
        scene.Init();
    }

    public void LeaveScene() {
        var scene = Scene;

        if (scene != null) {
            scene.Cleanup();
            Scene = scene.Parent;
        }
    }

    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    protected override void Draw(GameTime gameTime) {
        var scene = Scene;
        if (scene != null) {
            var t  = (float)gameTime.TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            scene.Draw(t, dt);
        }

        base.Draw(gameTime);
    }

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