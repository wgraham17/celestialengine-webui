namespace CelestialEngine.Extensions.WebUI
{
    using CefSharp.Internals;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Runtime.InteropServices;

    internal class Texture2DBitmapInfo : BitmapInfo
    {
        private byte[] buffer;
        private bool createNewBitmap;

        public Texture2DBitmapInfo()
        {
            this.BytesPerPixel = 4;
        }

        public override bool CreateNewBitmap
        {
            get
            {
                return this.createNewBitmap;
            }
        }

        public override void ClearBitmap()
        {
            this.createNewBitmap = true;
        }

        public void CopyBitmapToTexture2D(Texture2D target)
        {
            if (this.BackBufferHandle == IntPtr.Zero)
            {
                return;
            }

            if (this.createNewBitmap)
            {
                this.buffer = new byte[this.NumberOfBytes];
            }

            Marshal.Copy(this.BackBufferHandle, this.buffer, 0, this.buffer.Length);
            target.SetData(this.buffer);
        }
    }
}
