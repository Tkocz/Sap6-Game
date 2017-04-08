namespace GameName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;

using EngineName;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

public class TestScene: Scene {}

/// <summary>Provides a program entry point.</summary>
public static class Program {
    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Program entry point.</summary>
    /// <param name="args">The command line arguments.</param>
    [STAThread]
    private static void Main(string[] args) {
        // TODO: Create initial scene.
        using (var game = new Game1(new TestScene())) {
            game.Run();
        }
    }
}

}
