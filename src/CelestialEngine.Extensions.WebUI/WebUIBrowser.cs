using System;
using CefSharp;
using CefSharp.Internals;

namespace CelestialEngine.Extensions.WebUI
{
    internal class WebUIBrowser : IRenderWebBrowser
    {
        private ManagedCefBrowserAdapter managedCefBrowserAdapter;
        private IBrowser browser;
        private bool browserCreated;
        private BitmapFactory bitmapFactory;
        private Rect viewRect;
        
        #region Handlers

        public IDialogHandler DialogHandler { get; set; }
        public IRequestHandler RequestHandler { get; set; }
        public IDisplayHandler DisplayHandler { get; set; }
        public ILoadHandler LoadHandler { get; set; }
        public ILifeSpanHandler LifeSpanHandler { get; set; }
        public IKeyboardHandler KeyboardHandler { get; set; }
        public IJsDialogHandler JsDialogHandler { get; set; }
        public IDragHandler DragHandler { get; set; }
        public IDownloadHandler DownloadHandler { get; set; }
        public IContextMenuHandler MenuHandler { get; set; }
        public IFocusHandler FocusHandler { get; set; }
        public IResourceHandlerFactory ResourceHandlerFactory { get; set; }
        public IGeolocationHandler GeolocationHandler { get; set; }
        public IRenderProcessMessageHandler RenderProcessMessageHandler { get; set; }
        public IFindHandler FindHandler { get; set; }

        #endregion

        public IBrowserAdapter BrowserAdapter
        {
            get
            {
                return this.managedCefBrowserAdapter;
            }
        }

        public BrowserSettings BrowserSettings { get; private set; }

        public RequestContext RequestContext { get; private set; }

        public bool HasParent { get; set; }

        public bool IsBrowserInitialized { get; private set; }

        public bool IsLoading { get; private set; }

        public bool CanGoBack { get; private set; }

        public bool CanGoForward { get; private set; }

        public string Address { get; private set; }

        public string TooltipText { get; private set; }

        public bool CanExecuteJavascriptInMainFrame { get; private set; }

        public Texture2DBitmapInfo LastFrame { get; private set; }

        public event EventHandler<ConsoleMessageEventArgs> ConsoleMessage;
        public event EventHandler<StatusMessageEventArgs> StatusMessage;
        public event EventHandler<FrameLoadStartEventArgs> FrameLoadStart;
        public event EventHandler<FrameLoadEndEventArgs> FrameLoadEnd;
        public event EventHandler<LoadErrorEventArgs> LoadError;
        public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;

        public WebUIBrowser(int browserWidth, int browserHeight, string address = "", BrowserSettings browserSettings = null, RequestContext requestContext = null)
        {
            // Initialize CEF if needed. Really we should check if WebUISystem.Initialize has been called.
            if (!Cef.IsInitialized && !Cef.Initialize())
            {
                throw new InvalidOperationException("Cef::Initialize() failed");
            }

            this.viewRect = new Rect(0, 0, browserWidth, browserHeight);

            this.bitmapFactory = new BitmapFactory();

            this.ResourceHandlerFactory = new DefaultResourceHandlerFactory();
            this.LifeSpanHandler = new DisablePopupsLifeSpanHandler();
            this.MenuHandler = new DisableContextMenuHandler();

            this.Address = address;
            this.BrowserSettings = browserSettings ?? new BrowserSettings();
            this.RequestContext = requestContext;

            Cef.AddDisposable(this);

            managedCefBrowserAdapter = new ManagedCefBrowserAdapter(this, true);
        }

        public void CreateBrowser(IntPtr windowHandle)
        {
            if (this.browserCreated)
            {
                throw new Exception("An instance of the underlying offscreen browser has already been created, this method can only be called once.");
            }

            this.browserCreated = true;

            managedCefBrowserAdapter.CreateOffscreenBrowser(windowHandle, this.BrowserSettings, this.RequestContext, this.Address);
        }

        public BitmapInfo CreateBitmapInfo(bool isPopup)
        {
            return this.bitmapFactory.CreateBitmap(isPopup, 1.0);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IBrowser GetBrowser()
        {
            this.ThrowExceptionIfBrowserNotInitialized();
            return this.browser;
        }
        
        public ViewRect GetViewRect()
        {
            return new ViewRect(this.viewRect.Width, this.viewRect.Height);
        }

        public void InvokeRenderAsync(BitmapInfo bitmapInfo)
        {
            this.LastFrame = (Texture2DBitmapInfo)bitmapInfo;
            this.LastFrame.SetDirty();
        }

        public void Load(string url)
        {
            this.Address = url;

            using (var frame = this.GetMainFrame())
            {
                frame.LoadUrl(url);
            }
        }

        public void OnAfterBrowserCreated(IBrowser browser)
        {
            this.browser = browser;

            this.IsBrowserInitialized = true;
        }

        public void RegisterAsyncJsObject(string name, object objectToBind, BindingOptions options = null)
        {
            if (this.IsBrowserInitialized)
            {
                throw new Exception("Browser is already initialized. RegisterJsObject must be" +
                                    "called before the underlying CEF browser is created.");
            }

            this.managedCefBrowserAdapter.RegisterAsyncJsObject(name, objectToBind, options);
        }

        public void RegisterJsObject(string name, object objectToBind, BindingOptions options = null)
        {
            if (this.IsBrowserInitialized)
            {
                throw new Exception("Browser is already initialized. RegisterJsObject must be" +
                                    "called before the underlying browser is created.");
            }
            
            CefSharpSettings.WcfEnabled = true;
            this.managedCefBrowserAdapter.RegisterJsObject(name, objectToBind, options);
        }

        #region State Changes

        public void SetAddress(AddressChangedEventArgs args)
        {
            this.Address = args.Address;
        }

        public void SetCanExecuteJavascriptOnMainFrame(bool canExecute)
        {
            this.CanExecuteJavascriptInMainFrame = canExecute;
        }

        public void SetLoadingStateChange(LoadingStateChangedEventArgs args)
        {
            this.CanGoBack = args.CanGoBack;
            this.CanGoForward = args.CanGoForward;
            this.IsLoading = args.IsLoading;

            this.LoadingStateChanged?.Invoke(this, args);
        }

        public void SetTooltipText(string tooltipText)
        {
            this.TooltipText = tooltipText;
        }

        #endregion

        #region Event Invocation

        public void OnConsoleMessage(ConsoleMessageEventArgs args)
        {
            this.ConsoleMessage?.Invoke(this, args);
        }

        public void OnFrameLoadEnd(FrameLoadEndEventArgs args)
        {
            this.FrameLoadEnd?.Invoke(this, args);
        }

        public void OnFrameLoadStart(FrameLoadStartEventArgs args)
        {
            this.FrameLoadStart?.Invoke(this, args);
        }

        public void OnLoadError(LoadErrorEventArgs args)
        {
            this.LoadError?.Invoke(this, args);
        }

        public void OnStatusMessage(StatusMessageEventArgs args)
        {
            this.StatusMessage?.Invoke(this, args);
        }

        #endregion

        #region Unimplemented Behavior

        public bool Focus()
        {
            return false;
        }
        public ScreenInfo GetScreenInfo()
        {
            return new ScreenInfo(1.0f);
        }

        public bool GetScreenPoint(int viewX, int viewY, out int screenX, out int screenY)
        {
            screenX = 0;
            screenY = 0;

            return false;
        }

        public void OnImeCompositionRangeChanged(Range selectedRange, Rect[] characterBounds)
        {
        }

        public void SetCursor(IntPtr cursor, CursorType type)
        {
        }

        public void SetPopupIsOpen(bool show)
        {
        }

        public void SetPopupSizeAndPosition(int width, int height, int x, int y)
        {
        }

        public void SetTitle(TitleChangedEventArgs args)
        {
        }

        public bool StartDragging(IDragData dragData, DragOperationsMask mask, int x, int y)
        {
            return false;
        }

        public void UpdateDragCursor(DragOperationsMask operation)
        {
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            // Don't reference event listeners any longer:
            this.LoadError = null;
            this.FrameLoadStart = null;
            this.FrameLoadEnd = null;
            this.ConsoleMessage = null;
            this.StatusMessage = null;
            this.LoadingStateChanged = null;

            Cef.RemoveDisposable(this);

            if (disposing)
            {
                browser = null;
                IsBrowserInitialized = false;

                // Cleanup the browser render buffer
                //if (Bitmap != null)
                //{
                //    Bitmap.Dispose();
                //    Bitmap = null;
                //}

                if (this.BrowserSettings != null)
                {
                    this.BrowserSettings.Dispose();
                    this.BrowserSettings = null;
                }

                if (managedCefBrowserAdapter != null)
                {
                    if (!managedCefBrowserAdapter.IsDisposed)
                    {
                        managedCefBrowserAdapter.Dispose();
                    }
                    managedCefBrowserAdapter = null;
                }
            }

            // Release reference to handlers, make sure this is done after we dispose managedCefBrowserAdapter
            // otherwise the ILifeSpanHandler.DoClose will not be invoked.
            this.DialogHandler = null;
            this.RequestHandler = null;
            this.DisplayHandler = null;
            this.LoadHandler = null;
            this.LifeSpanHandler = null;
            this.KeyboardHandler = null;
            this.JsDialogHandler = null;
            this.DragHandler = null;
            this.DownloadHandler = null;
            this.MenuHandler = null;
            this.FocusHandler = null;
            this.ResourceHandlerFactory = null;
            this.GeolocationHandler = null;
            this.RenderProcessMessageHandler = null;
        }

        #region Handler Classes

        private class DisablePopupsLifeSpanHandler : ILifeSpanHandler
        {
            public bool DoClose(IWebBrowser browserControl, IBrowser browser)
            {
                return false;
            }

            public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
            {
            }

            public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
            {
            }

            public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                newBrowser = null;
                return true;
            }
        }

        private class DisableContextMenuHandler : IContextMenuHandler
        {
            public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
            }

            public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                return true;
            }

            public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
            {
            }

            public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
            {
                return true;
            }
        }

        #endregion
    }
}