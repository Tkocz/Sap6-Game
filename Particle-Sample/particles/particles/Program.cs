using System;

namespace Particle3DSample {
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Particle3DSampleGame game = new Particle3DSampleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

