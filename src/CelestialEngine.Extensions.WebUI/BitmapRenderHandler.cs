namespace CelestialEngine.Extensions.WebUI
{
    using CefSharp;
    using CefSharp.Enums;
    using CefSharp.OffScreen;
    using CefSharp.Structs;
    using System;
    using System.Threading;

    internal class BitmapRenderHandler : IRenderHandler
    {
        public readonly object BitmapLock = new object();
        private readonly Rect viewRect;
        private int dirtyState;

        public BitmapRenderHandler(int width, int height)
        {
            this.viewRect = new Rect(0, 0, width, height);
            this.BitmapBuffer = new BitmapBuffer(this.BitmapLock);
        }

        /// <summary>
        /// Contains the last bitmap buffer. Direct access
        /// to the underlying buffer - there is no locking when trying
        /// to access directly, use <see cref="BitmapBuffer.BitmapLock" /> where appropriate.
        /// </summary>
        /// <value>The bitmap.</value>
        public BitmapBuffer BitmapBuffer { get; private set; }

        public void Dispose()
        {
        }

        public ScreenInfo? GetScreenInfo()
        {
            var screenInfo = new ScreenInfo { DeviceScaleFactor = 1.0F };
            return screenInfo;
        }

        public bool GetScreenPoint(int viewX, int viewY, out int screenX, out int screenY)
        {
            screenX = viewX;
            screenY = viewY;

            return false;
        }

        public Rect GetViewRect() => this.viewRect;

        public void OnAcceleratedPaint(PaintElementType type, Rect dirtyRect, IntPtr sharedHandle)
        {
        }

        public void OnCursorChange(IntPtr cursor, CursorType type, CursorInfo customCursorInfo)
        {
        }

        public void OnImeCompositionRangeChanged(Range selectedRange, Rect[] characterBounds)
        {
        }

        public void OnPaint(PaintElementType type, Rect dirtyRect, IntPtr buffer, int width, int height)
        {
            if (type == PaintElementType.View)
            {
                while (Interlocked.CompareExchange(ref this.dirtyState, 1, 0) == 0) ;
                this.BitmapBuffer.UpdateBuffer(width, height, buffer, dirtyRect);
            }
        }

        public void OnPopupShow(bool show)
        {
        }

        public void OnPopupSize(Rect rect)
        {
        }

        public void OnVirtualKeyboardRequested(IBrowser browser, TextInputMode inputMode)
        {
        }

        public bool StartDragging(IDragData dragData, DragOperationsMask mask, int x, int y)
        {
            return false;
        }

        public void UpdateDragCursor(DragOperationsMask operation)
        {
        }

        internal bool GetDirtyStateDestructive()
        {
            return Interlocked.CompareExchange(ref this.dirtyState, 0, 1) == 1;
        }
    }
}
