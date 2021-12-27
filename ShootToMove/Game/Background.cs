using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShootToMove
{
    class Background
    {
        private Level level;
        private Player player;
        private Camera camera;
        private Vector2 worldPosition;
        private float positionFactor; //affects how background moves in relation to camera to create parallaxing effect
        private Texture2D texture;
        private bool menu; //menus just need a static, non-moving background

        public Background(bool menu)
        {
            this.menu = menu;
            worldPosition = new Vector2(0, 0);
        }

        public Background(Level level, float positionFactor)
        {
            this.level = level;
            this.positionFactor = positionFactor;
            camera = level.camera;
            player = level.GetPlayer();
            menu = false;
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Sprites/Backgrounds/background");
        }

        public void Update(GameTime gameTime)
        {
            if (!menu) worldPosition = camera.GetWorldPosition() * positionFactor; //creates parallaxing effect
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = -25; i < 50; i++) //tile the background sprite
            {
                for (int j = -25; j < 50; j++)
                {
                    spriteBatch.Draw(texture, new Vector2(worldPosition.X + (128 * j), worldPosition.Y + (128 * i)), Color.White);
                }
            }
        }
    }
}
