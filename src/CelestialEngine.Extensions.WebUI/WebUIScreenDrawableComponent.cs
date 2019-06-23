namespace CelestialEngine.Extensions.WebUI
{
    using CefSharp;
    using CefSharp.OffScreen;
    using CelestialEngine.Core;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class WebUIScreenDrawableComponent : ScreenDrawableComponent
    {
        private int browserWidth;
        private int browserHeight;
        private Vector2 position;
        private string startPage;

        private ConcurrentQueue<char> pendingChars;
        private BrowserSettings browserSettings;
        private ChromiumWebBrowser browser;
        private BitmapRenderHandler renderHandler;
        private WebUIMessageBusSink messageBusSink;
        private Texture2D browserTexture;
        private ConditionalInputBinding inputBindingRegistration;
        private Dictionary<string, Action<string>> messageCallbacks;
        private bool canHandleInput;

        public WebUIScreenDrawableComponent(World world, int browserWidth, int browserHeight, Vector2 position, int browserFps = 30, string startPage = "index.html")
            : base(world)
        {
            this.position = position;
            this.browserWidth = browserWidth;
            this.browserHeight = browserHeight;
            this.startPage = startPage;
            this.browserSettings = new BrowserSettings()
            {
                BackgroundColor = 0,
                WindowlessFrameRate = browserFps
            };

            this.messageBusSink = new WebUIMessageBusSink();
            this.pendingChars = new ConcurrentQueue<char>();
            this.messageCallbacks = new Dictionary<string, Action<string>>();

            this.inputBindingRegistration = ((BaseGame)this.World.Game).InputManager.AddBinding((s) => this.HandleInput(s));
            this.World.Game.Window.TextInput += GameWindowTextInput;

            //this.browser = new WebUIBrowser(this.browserWidth, this.browserHeight, $"webui://game/{this.startPage}", browserSettings: this.browserSettings);
            this.renderHandler = new BitmapRenderHandler(this.browserWidth, this.browserHeight);
            this.browser = new ChromiumWebBrowser($"webui://game/{this.startPage}", this.browserSettings, null, false)
            {
                MenuHandler = new NoMenuHandler(),
                RenderHandler = this.renderHandler
            };
            this.browser.JavascriptObjectRepository.Register("webUIMessage", this.messageBusSink, true);
            this.browser.CreateBrowser(new WindowInfo() { Width = this.browserWidth, Height = this.browserHeight, WindowHandle = IntPtr.Zero, WindowlessRenderingEnabled = true});
            this.canHandleInput = true;
        }

        public override void Draw(GameTime gameTime, ScreenSpriteBatch spriteBatch)
        {
            if (this.renderHandler.GetDirtyStateDestructive())
            {
                lock (this.renderHandler.BitmapLock)
                {
                    this.browserTexture.SetData(this.renderHandler.BitmapBuffer.Buffer);
                }
            }

            spriteBatch.Draw(this.browserTexture, this.position, null, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None);
        }

        public override void LoadContent(ExtendedContentManager contentManager)
        {
            this.browserTexture = new Texture2D(this.World.Game.GraphicsDevice, this.browserWidth, this.browserHeight, false, SurfaceFormat.Bgra32);
        }

        private void GameWindowTextInput(object sender, TextInputEventArgs e)
        {
            if (!char.IsControl(e.Character))
            {
                this.pendingChars.Enqueue(e.Character);
            }
        }

        public override void Update(GameTime gameTime)
        {
            var pendingMessages = this.messageBusSink.GetAllAndFlush();

            foreach (var message in pendingMessages)
            {
                this.messageCallbacks[message.Name]?.Invoke(message.Data);
            }
        }

        public void PushEventToBrowser<T>(string name, T data)
        {
            var container = JsonConvert.SerializeObject(new { name = name, data = data });
            this.browser.GetMainFrame().ExecuteJavaScriptAsync($"var _msgobj = JSON.parse('{container}'); if (typeof window.webUICallbacks !== 'undefined' && typeof window.webUICallbacks[_msgobj.name] === 'function') window.webUICallbacks[_msgobj.name](_msgobj.data);");
        }

        public void RegisterEventCallback(string eventName, Action<string> handler)
        {
            this.messageCallbacks[eventName] = handler;
        }

        public void UnregisterEventCallback(string eventName)
        {
            this.messageCallbacks.Remove(eventName);
        }

        public override void Dispose()
        {
            this.World.Game.Window.TextInput -= this.GameWindowTextInput;
            this.browser.Dispose();
        }

        private void HandleInput(InputState state)
        {
            if (!this.canHandleInput || !this.Enabled)
            {
                return;
            }

            var browserRect = new RectangleF(this.position.X, this.position.Y, this.browserWidth, this.browserHeight);
            var host = this.browser.GetBrowser().GetHost();

            // Input state detection
            var relativeMousePosition = state.CurrentMouseState.Position.ToVector2() - this.position;
            var didMouseLeave = (!browserRect.Contains(state.CurrentMouseState.Position.ToVector2()) && browserRect.Contains(state.LastMouseState.Position.ToVector2()));
            var lastKeysDown = state.LastKeyboardState.GetPressedKeys();
            var keysDown = state.CurrentKeyboardState.GetPressedKeys();

            // Create the bitflags for CEF events
            var eventFlags = this.GetFullEventFlags(state);
            var kbEventFlags = (int)this.GetKeyboardEventFlags(eventFlags);

            // Mouse move event tracking
            if (state.LastMouseState.Position != state.CurrentMouseState.Position)
            {
                host.SendMouseMoveEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, didMouseLeave, eventFlags);
            }

            // Click and mousewheel scroll tracking only if we're in bounds on the browser
            if (browserRect.Contains(state.CurrentMouseState.Position.ToVector2()))
            {
                this.DispatchClickEvent(host, state.CurrentMouseState.LeftButton, state.LastMouseState.LeftButton, relativeMousePosition, MouseButtonType.Left, eventFlags);
                this.DispatchClickEvent(host, state.CurrentMouseState.MiddleButton, state.LastMouseState.MiddleButton, relativeMousePosition, MouseButtonType.Middle, eventFlags);
                this.DispatchClickEvent(host, state.CurrentMouseState.RightButton, state.LastMouseState.RightButton, relativeMousePosition, MouseButtonType.Right, eventFlags);

                if (state.IsScrollWheelChanged())
                {
                    var delta = state.CurrentMouseState.ScrollWheelValue - state.LastMouseState.ScrollWheelValue;
                    host.SendMouseWheelEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, 0, delta, eventFlags);
                }
            }

            // Compute WM_KEYDOWN and WM_KEYUP events
            var keysReleased = lastKeysDown.Except(keysDown);
            var keysPressed = keysDown.Except(lastKeysDown);

            foreach (var key in keysReleased)
            {
                host.SendKeyEvent((int)WM.KEYUP, (int)key, kbEventFlags);
            }

            foreach (var key in keysPressed)
            {
                host.SendKeyEvent((int)WM.KEYDOWN, (int)key, kbEventFlags);
            }

            // Process any WM_CHAR events received from MonoGame
            while ((this.pendingChars.TryDequeue(out char c)))
            {
                host.SendKeyEvent((int)WM.CHAR, (int)c, kbEventFlags);
            }

            if (state.IsFirstKeyPress(Microsoft.Xna.Framework.Input.Keys.F12))
            {
                host.ShowDevTools();
            }
        }

        private CefEventFlags GetFullEventFlags(InputState state)
        {
            CefEventFlags eventFlags = CefEventFlags.None;

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
            {
                eventFlags |= CefEventFlags.AltDown | CefEventFlags.IsLeft;
            }

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt))
            {
                eventFlags |= CefEventFlags.AltDown | CefEventFlags.IsRight;
            }

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                eventFlags |= CefEventFlags.ControlDown | CefEventFlags.IsLeft;
            }

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                eventFlags |= CefEventFlags.ControlDown | CefEventFlags.IsRight;
            }

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                eventFlags |= CefEventFlags.ShiftDown | CefEventFlags.IsLeft;
            }

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                eventFlags |= CefEventFlags.ShiftDown | CefEventFlags.IsRight;
            }

            if (state.IsLeftMouseDown())
            {
                eventFlags |= CefEventFlags.LeftMouseButton;
            }

            if (state.IsRightMouseDown())
            {
                eventFlags |= CefEventFlags.RightMouseButton;
            }

            return eventFlags;
        }

        private CefEventFlags GetKeyboardEventFlags(CefEventFlags inputFlags)
        {
            return inputFlags & (CefEventFlags.AltDown | CefEventFlags.ControlDown | CefEventFlags.ShiftDown);
        }

        private void DispatchClickEvent(IBrowserHost host, Microsoft.Xna.Framework.Input.ButtonState currentState, Microsoft.Xna.Framework.Input.ButtonState lastState, Vector2 relativeMousePosition, MouseButtonType mouseButtonType, CefEventFlags eventFlags)
        {
            if (lastState == Microsoft.Xna.Framework.Input.ButtonState.Released && currentState == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                host.SendMouseClickEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, mouseButtonType, false, 1, eventFlags);
            }
            else if (lastState == Microsoft.Xna.Framework.Input.ButtonState.Pressed && currentState == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                host.SendMouseClickEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, mouseButtonType, true, 1, eventFlags);
            }
        }
    }
}
