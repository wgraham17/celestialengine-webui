namespace CelestialEngine.Extensions.WebUI.Test
{
    using CelestialEngine.Core;
    using Microsoft.Xna.Framework;
    using System;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : BaseGame
    {
        private WebUIScreenDrawableComponent webUIComponent;
        private float secFrames;
        private double elapsed;
        private double lastFramesPerSec;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyGame"/> class.
        /// </summary>
        public TestGame()
        {
            Window.Title = "Celestial Engine - WebUI Extension Test";
            this.IsMouseVisible = true;
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            WebUISystem.Shutdown();
            base.OnExiting(sender, args);
        }

        /// <summary>
        /// Called after the Game and GraphicsDevice are created, but before LoadContent.  Reference page contains code sample.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize the camera and/or input manager as needed
            WebUISystem.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content for the game or screen
            this.webUIComponent = new WebUIScreenDrawableComponent(this.GameWorld, 1280, 720, Vector2.Zero);
            base.LoadContent();
        }

        /// <summary>
        /// Draws the specified game time.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        protected override void Draw(GameTime gameTime)
        {
            Window.Title = String.Format("Celestial Engine - WebUI Extension Test [{0} FPS(real), {1} FPS(avg)]", Math.Round(1000.0f / gameTime.ElapsedGameTime.TotalMilliseconds), lastFramesPerSec);

            base.Draw(gameTime);

            secFrames++;
            elapsed += gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsed > 1.0)
            {
                lastFramesPerSec = Math.Round(secFrames / elapsed);
                elapsed = 0;
                secFrames = 0;
            }
        }
    }
}
