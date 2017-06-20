namespace CelestialEngine.Extensions.WebUI
{
    using CefSharp;
    using CelestialEngine.Core;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class WebUIScreenDrawableComponent : ScreenDrawableComponent
    {
        private int browserWidth;
        private int browserHeight;
        private Vector2 position;

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
            if (this.browser.LastFrame != null)
            {
                this.browser.LastFrame.CopyBitmapToTexture2D(this.browserTexture);
                spriteBatch.Draw(this.browserTexture, this.position, null, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None);
            }
        }

        public override void LoadContent(ExtendedContentManager contentManager)
        {
            this.inputBindingRegistration = ((BaseGame)this.World.Game).InputManager.AddBinding((s) => this.HandleInput(s));
            this.browserTexture = new Texture2D(this.World.Game.GraphicsDevice, this.browserWidth, this.browserHeight, false, SurfaceFormat.Bgra32);
            this.browser = new WebUIBrowser(this.browserWidth, this.browserHeight, "https://hulshofschmidt.files.wordpress.com/2011/05/color_test_pattern.jpg", browserSettings: this.browserSettings);
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
            var keysDown = state.CurrentKeyboardState.GetPressedKeys();
            var eventFlags = this.GetEventFlags(state);

            if (state.LastMouseState.Position != state.CurrentMouseState.Position)
            {
                host.SendMouseMoveEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, didMouseLeave, eventFlags);
            }

            if (state.IsScrollWheelChanged())
            {
                host.SendMouseWheelEvent((int)relativeMousePosition.X, (int)relativeMousePosition.Y, state.CurrentMouseState.ScrollWheelValue, 0, eventFlags);
            }
        }

        public override void Dispose()
        {
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
    }
}
