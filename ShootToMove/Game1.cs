using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace ShootToMove
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;

        //These values effect the resolution
        public float width = 1280;
        public float height = 720;
        private Song song;

        Level level;
        Menu menu;
        
        public Game1()
        {
            //set the default graphics options on startup
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = (int)width;
            graphics.PreferredBackBufferHeight = (int)height;

            //Where the assets are located
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //level must be initialised so that it has access to the necessary variables and objects
            //this is not very efficient but I only realised towards the end of development and fixing this
            //would require a lot of the level class to be rewritten and subsequently many of the other classes
            level = new Level(Level.LevelID.level1, this, Content, 0);
            menu = new Menu(this, level);
            level.menu = menu;

            //game is rendered to a 1080p screen and is scaled to fit the other resolutions. The renderTarget acts as this virtual screen
            renderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            menu.LoadContent(Content);
            level.LoadContent(Content);
            song = Content.Load<Song>("Music/mainMusic");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.15f;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            if (menu.play) level.Update(gameTime);
            else menu.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //first draw to the render target
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.DimGray); //clears the last frame

            // TODO: Add your drawing code here
            //the camera matrix is passed to the spritebatch so that it can apply the matrix transformation to everything it draws
            if (menu.play)
            {
                spriteBatch.Begin(0, null, null, null, null, null, level.camera.GetCameraMatrix());
                level.Draw(spriteBatch);
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(0, null, null, null, null, null, menu.camera.GetCameraMatrix());
                menu.Draw(spriteBatch);
                spriteBatch.End();
            }

            //this drops the render target and allows the graphics device to switch back to the screen
            GraphicsDevice.SetRenderTarget(null);

            //finally draw the render target to the screen
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, (int)width, (int)height), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void ToggleFullscreen()
        {
            graphics.IsFullScreen = !graphics.IsFullScreen;
            graphics.ApplyChanges();
        }

        public void ChangeResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();
        }

        public void FinalLevelCompleted() //used when player collides with levelEnd sign in final level. Switches game to menu mode
        {
            menu.play = false;
        }
    }
}
