namespace CelestialEngine.Extensions.WebUI
{
    using CefSharp;
    using CefSharp.Internals;

    internal class BitmapFactory : IBitmapFactory
    {
        public BitmapInfo CreateBitmap(bool isPopup, double dpiScale)
        {
            return new Texture2DBitmapInfo();
        }
    }
}
