using GameName.Dev;

namespace GameName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;

using Thengill;
using Thengill.Logging;
using Thengill.Components;
using Thengill.Components.Renderable;
using Thengill.Systems;

using GameName.Scenes;
    using GameName.Dev;

    /*--------------------------------------
     * CLASSES
     *------------------------------------*/

    /// <summary>Provides a program entry point.</summary>
    public static class Game {
    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Program entry point.</summary>
    /// <param name="args">The command line arguments.</param>
    [STAThread]
    private static void Main(string[] args) {
        Log.ToFile();

        // TODO: Create initial scene.
	    using (var game = new Game1(new MainMenu(args))) {
            game.Run();
        }

        // This point is apparently never reached because MonoGame force quits
        // the process intead of returning...
    }
}

}
