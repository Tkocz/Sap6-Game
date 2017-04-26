namespace GameName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;

using EngineName;
using EngineName.Logging;
    using EngineName.Components;
    using EngineName.Components.Renderable;
    using EngineName.Systems;
    using GameName.Scenes;


    /// <summary>Provides a program entry point.</summary>
    public static class Game {
    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Program entry point.</summary>
    /// <param name="args">The command line arguments.</param>
#if !philip
    [STAThread]
    private static void Main(string[] args) {
        Log.ToFile();

        // TODO: Create initial scene.
			using (var game = new Game1(new WorldScene())) {
            game.Run();
        }

        // This point is apparently never reached because MonoGame force quits
        // the process intead of returning...
    }
#endif
}

}
