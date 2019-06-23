namespace CelestialEngine.Extensions.WebUI.Demo
{
    using CelestialEngine.Core;
    using CelestialEngine.Game;
    using CelestialEngine.Game.PostProcess.Lights;
    using Microsoft.Xna.Framework;
    using System;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class DemoGame : BaseGame
    {
        private WebUIScreenDrawableComponent webUIComponent;
        private float secFrames;
        private double elapsed;
        private double lastFramesPerSec;
        private TiledSprite backgroundSprite;
        private Random prng = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="MyGame"/> class.
        /// </summary>
        public DemoGame()
        {
            Window.Title = "Celestial Engine - WebUI Extension Demo";
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
            WebUISystem.Initialize(this.Content.RootDirectory);
            base.Initialize();
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content for the game or screen
            this.webUIComponent = new WebUIScreenDrawableComponent(this.GameWorld, 1280, 720, Vector2.Zero);
            this.webUIComponent.RegisterEventCallback("game:spawnLight", this.SpawnLight);

            this.backgroundSprite = new TiledSprite(this.GameWorld, "Content/stone", null, Vector2.Zero, new Vector2(this.GraphicsDeviceManager.PreferredBackBufferWidth, this.GraphicsDeviceManager.PreferredBackBufferHeight) * this.GameWorld.WorldPerPixelRatio)
            {
                LayerDepth = 1,
                RenderOptions = SpriteRenderOptions.IsLit
            };

            var newLight = new BouncyPointLight(this.GameWorld)
            {
                Position = new Vector3(Vector2.Zero, 0.15f),
                Velocity = this.GetRandomVelocity(),
                Color = this.GetRandomColor(),
                Power = 0.25f,
                Range = this.prng.Next(300, 500) / 100.0f,
                SpecularStrength = 2.75f,
                CastsShadows = true,
                LayerDepth = 2
            };

            var amLight = new AmbientLight(Color.White, 0.2f, true, 1);
            
            this.RenderSystem.AddPostProcessEffect(newLight);
            this.RenderSystem.AddPostProcessEffect(amLight);

            this.GameCamera.Position = new Vector2(this.GraphicsDeviceManager.PreferredBackBufferWidth, this.GraphicsDeviceManager.PreferredBackBufferHeight) * this.GameWorld.WorldPerPixelRatio * 0.5f;

            base.LoadContent();
        }

        /// <summary>
        /// Draws the specified game time.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        protected override void Draw(GameTime gameTime)
        {
            Window.Title = String.Format("Celestial Engine - WebUI Extension Demo [{0} FPS(real), {1} FPS(avg)]", Math.Round(1000.0f / gameTime.ElapsedGameTime.TotalMilliseconds), lastFramesPerSec);

            base.Draw(gameTime);

            secFrames++;
            elapsed += gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsed > 1.0)
            {
                lastFramesPerSec = Math.Round(secFrames / elapsed);
                this.webUIComponent.PushEventToBrowser("game:fps", lastFramesPerSec.ToString());
                elapsed = 0;
                secFrames = 0;
            }
        }

        private void SpawnLight(string data)
        {
            var newLight = new BouncyPointLight(this.GameWorld)
            {
                Position = new Vector3(Vector2.Zero, 0.15f),
                Velocity = this.GetRandomVelocity(),
                Color = this.GetRandomColor(),
                Power = 0.25f,
                Range = this.prng.Next(300, 500) / 100.0f,
                SpecularStrength = 2.75f,
                CastsShadows = true,
                LayerDepth = 2
            };
            this.RenderSystem.AddPostProcessEffect(newLight);
        }

        private Color GetRandomColor()
        {
            int f = 0;
            int s = 0;
            int t = 0;

            while (Math.Sqrt(s * s + f * f + t * t) < 250)
            {
                f = this.prng.Next(0, 255);
                s = this.prng.Next(0, 255);
                t = this.prng.Next(0, 255);
            }

            return new Color(f, s, t);
        }

        private Vector3 GetRandomVelocity()
        {
            Vector3 currVal;

            do
            {
                currVal = new Vector3(this.prng.Next(-400, 400) / 100.0f, this.prng.Next(-400, 400) / 100.0f, 0);
            } while (currVal.Length() < 1.5f);

            return currVal;
        }
    }
}
