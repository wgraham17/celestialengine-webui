using System;

namespace CelestialEngine.Extensions.WebUI.Demo
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
            using (var game = new DemoGame())
            {
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.GraphicsDeviceManager.ApplyChanges();

                game.IsFixedTimeStep = false;
                game.Run();
            }
        }
    }
#endif
}
