using System;

namespace CelestialEngine.Extensions.WebUI.Test
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new TestGame())
            {
                game.GraphicsDeviceManager.PreferredBackBufferHeight = 720;
                game.GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.GraphicsDeviceManager.ApplyChanges();

                game.IsFixedTimeStep = false;

                game.Run();
            }
        }
    }
#endif
}
