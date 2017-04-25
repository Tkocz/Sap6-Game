namespace GameName {

#if philip

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;

using EngineName;
using EngineName.Logging;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Systems;

using Dev;

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

        using (var game = new Game1(new CollTestScene2())) {
            game.Run();
        }

        // This point is apparently never reached because MonoGame force quits
        // the process intead of returning...
    }
}

#endif

}