namespace CelestialEngine.Extensions.WebUI
{
    using CefSharp;
    using CelestialEngine.Core;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Collections.Concurrent;
    using System.Linq;

    public class WebUIScreenDrawableComponent : ScreenDrawableComponent
    {
        private int browserWidth;
        private int browserHeight;
        private Vector2 position;

        private ConcurrentQueue<char> pendingChars;
        private BrowserSettings browserSettings;
        private WebUIBrowser browser;
        private Texture2D browserTexture;
        private ConditionalInputBinding inputBindingRegistration;

        public WebUIScreenDrawableComponent(World world, int browserWidth, int browserHeight, Vector2 position, int browserFps = 30)
            : base(world)
        {
            this.position = position;
            this.browserWidth = browserWidth;
            this.browserHeight = browserHeight;
            this.browserSettings = new BrowserSettings()
            {
                OffScreenTransparentBackground = true,
                WindowlessFrameRate = browserFps
            };
        }

        public override void Draw(GameTime gameTime, ScreenSpriteBatch spriteBatch)
        {
            if (this.browser.LastFrame != null && this.browser.LastFrame.GetDirtyStateDestructive())
            {
                this.browser.LastFrame.CopyBitmapToTexture2D(this.browserTexture);
            }

            spriteBatch.Draw(this.browserTexture, this.position, null, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None);
        }

        public override void LoadContent(ExtendedContentManager contentManager)
        {
            this.inputBindingRegistration = ((BaseGame)this.World.Game).InputManager.AddBinding((s) => this.HandleInput(s));
            this.browserTexture = new Texture2D(this.World.Game.GraphicsDevice, this.browserWidth, this.browserHeight, false, SurfaceFormat.Bgra32);
            this.browser = new WebUIBrowser(this.browserWidth, this.browserHeight, "https://google.com/", browserSettings: this.browserSettings);

            this.pendingChars = new ConcurrentQueue<char>();
            this.World.Game.Window.TextInput += Window_TextInput;
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            this.pendingChars.Enqueue(e.Character);
        }

        public override void Update(GameTime gameTime)
        {
        }

        public void HandleInput(InputState state)
        {
            var browserRect = new RectangleF(this.position.X, this.position.Y, this.browserWidth, this.browserHeight);
            var host = this.browser.GetBrowser().GetHost();

            // Input state detection
            var relativeMousePosition = state.CurrentMouseState.Position.ToVector2() - this.position;
            var didMouseLeave = (!browserRect.Contains(state.CurrentMouseState.Position.ToVector2()) && browserRect.Contains(state.LastMouseState.Position.ToVector2()));
            var lastKeysDown = state.LastKeyboardState.GetPressedKeys();
            var keysDown = state.CurrentKeyboardState.GetPressedKeys();
            var eventFlags = this.GetEventFlags(state);

            if (state.LastMouseState.Position != state.CurrentMouseState.Position)
            {
                host.SendMouseMoveEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, didMouseLeave, eventFlags);
            }

            if (browserRect.Contains(state.CurrentMouseState.Position.ToVector2()))
            {
                this.HandleClickEvent(host, state.CurrentMouseState.LeftButton, state.LastMouseState.LeftButton, relativeMousePosition, MouseButtonType.Left, eventFlags);
                this.HandleClickEvent(host, state.CurrentMouseState.MiddleButton, state.LastMouseState.MiddleButton, relativeMousePosition, MouseButtonType.Middle, eventFlags);
                this.HandleClickEvent(host, state.CurrentMouseState.RightButton, state.LastMouseState.RightButton, relativeMousePosition, MouseButtonType.Right, eventFlags);

                if (state.IsScrollWheelChanged())
                {
                    var delta = state.CurrentMouseState.ScrollWheelValue - state.LastMouseState.ScrollWheelValue;
                    host.SendMouseWheelEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, 0, delta, eventFlags);
                }
            }

            var keysReleased = lastKeysDown.Except(keysDown);
            var keysPressed = keysDown.Except(lastKeysDown);
            var kbFlags = (int)this.GetKeyboardEventFlags(state);

            foreach (var key in keysReleased)
            {
                host.SendKeyEvent((int)WM.KEYUP, (int)key, kbFlags);
            }

            foreach (var key in keysPressed)
            {
                host.SendKeyEvent((int)WM.KEYDOWN, (int)key, kbFlags);
            }

            while ((this.pendingChars.TryDequeue(out char c)))
            {
                host.SendKeyEvent((int)WM.CHAR, (int)c, kbFlags);
            }
        }

        public override void Dispose()
        {
            this.World.Game.Window.TextInput -= Window_TextInput;
            this.browser.Dispose();
        }

        private CefEventFlags GetEventFlags(InputState state)
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

        private CefEventFlags GetKeyboardEventFlags(InputState state)
        {
            CefEventFlags eventFlags = CefEventFlags.None;

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt))
            {
                eventFlags |= CefEventFlags.AltDown;
            }
            
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl))
            {
                eventFlags |= CefEventFlags.ControlDown;
            }

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
            {
                eventFlags |= CefEventFlags.ShiftDown;
            }
            
            return eventFlags;
        }

        private void HandleClickEvent(IBrowserHost host, Microsoft.Xna.Framework.Input.ButtonState currentState, Microsoft.Xna.Framework.Input.ButtonState lastState, Vector2 relativeMousePosition, MouseButtonType mouseButtonType, CefEventFlags eventFlags)
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
