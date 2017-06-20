namespace CelestialEngine.Extensions.WebUI
{
    using CefSharp.Internals;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class Texture2DBitmapInfo : BitmapInfo
    {
        private int dirtyState;
        private byte[] buffer;
        private bool createNewBitmap;

        public Texture2DBitmapInfo()
        {
            this.BytesPerPixel = 4;
        }

        internal void SetDirty()
        {
            while (Interlocked.CompareExchange(ref this.dirtyState, 1, 0) == 0) ;
        }

        internal bool GetDirtyStateDestructive()
        {
            return Interlocked.CompareExchange(ref this.dirtyState, 0, 1) == 1;
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
