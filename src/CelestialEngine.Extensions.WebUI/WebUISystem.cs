using CefSharp;

namespace CelestialEngine.Extensions.WebUI
{
    public static class WebUISystem
    {
        /// <summary>
        ///     Initializes WebUI with default settings. It's important to note that Initialize and 
        ///     Shutdown MUST be called on your main applicaiton thread (Typically the UI thead). If 
        ///     you call them on different threads, your application will hang.
        ///    See the documentation for WebUISystem.Shutdown() for more details.
        /// </summary>
        public static void Initialize()
        {
            var settings = new CefSettings();

            settings.SetOffScreenRenderingBestPerformanceArgs();
            //settings.UserAgent = "Mozilla/5.0 Chrome/57.0.2987.133 CelestialEngine-WebUI/1.0";
            //settings.CefCommandLineArgs.Add("renderer-process-limit", "1");
            //settings.CefCommandLineArgs.Add("disable-extensions", "1");
            //settings.CefCommandLineArgs.Add("disable-pdf-extension", "1");
            //settings.CefCommandLineArgs.Add("disable-plugins-discovery", "1");
            //settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1");

            settings.WindowlessRenderingEnabled = true;

            Cef.Initialize(settings);
        }

        /// <summary>
        ///     Shuts down WebUI and the underlying browser infrastructure. This method is safe
        ///     to call multiple times; it will only shut down WebUI on the first call (all subsequent
        ///     calls will be ignored). This method should be called on the main application
        ///     thread to shut down the WebUI browser processes before the application exits. You must call 
        ///     this explicitly before your game exits or it will hang. This method must be called on the same thread
        ///     as Initialize.
        /// </summary>
        public static void Shutdown()
        {
            Cef.Shutdown();
        }
    }
}
