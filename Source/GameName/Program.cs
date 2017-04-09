namespace GameName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;

using EngineName;
using EngineName.Logging;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

// TODO: Remove this crap.
public class TestScene: Scene {
    public override void Init() {
        base.Init();

        Log.Get().Debug("TestScene initialized.");
    }
}

/// <summary>Provides a program entry point.</summary>
public static class Program {
    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Program entry point.</summary>
    /// <param name="args">The command line arguments.</param>
    [STAThread]
    private static void Main(string[] args) {
        Log.ToFile();

        // TODO: Create initial scene.
        using (var game = new Game1(new TestScene())) {
            game.Run();
        }

        // This point is apparently never reached because MonoGame force quits
        // the process intead of returning...
    }
}

}
