using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ShootToMove
{
    class Camera
    {
        private Vector2 worldPosition;
        private Vector2 targetPosition;
        private Vector2 offset; //camera wont be centred on player instead it will be slightly above them
        private Vector2 screenCentre;
        private Vector2 translation;
        public Vector2 mousePosition;
        private const float smooth = 3f;
        private Game1 game1;
        private Player player;
        private Matrix cameraMatrix;
        public bool menu; //camera works different in menus

        public Camera(Player player, Game1 game1)
        {
            screenCentre = new Vector2(960, 540); //game rendered at 1080p and downscaled or upscaled depending on resolution
            offset = new Vector2(32, 64);
            this.player = player;
            this.game1 = game1;
            worldPosition = player.GetWorldPosition();
        }

        public Camera(bool menu, Game1 game1)
        {
            this.menu = menu;
            this.game1 = game1;
            screenCentre = new Vector2(960, 540);
            worldPosition = new Vector2(0, 0);
        }

        public void Update(GameTime gameTime)
        {
            if (!menu)
            {
                if (player != null && player.Alive())
                {
                    targetPosition = player.GetWorldPosition();
                    //lerp is a smoothing function that takes in two vectors and outputs a smoothed vector that is inbetween them
                    worldPosition = Vector2.Lerp(worldPosition, targetPosition, smooth * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
            translation = -worldPosition + screenCentre; //everything in the game world most be rendered in a different position
            //the translation vector describes the translation required to make it appear that the camera is moving in relation
            //to everything else
            cameraMatrix = Matrix.CreateTranslation(translation.X, translation.Y, 0);
            //cameraMatrix is the translation vector converted into a matrix. This can be supplied to the games draw method
            //to move all the sprites in the level accordingly

            //get mouse position. it must be converted to world space by transforming it by the inverse of the cameraMatrix
            MouseState mouseState = Mouse.GetState();
            mousePosition = new Vector2(mouseState.X * (1920 / game1.width), mouseState.Y * (1080 / game1.height));
            mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(cameraMatrix));
        }

        public Matrix GetCameraMatrix()
        {
            return cameraMatrix;
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        public Player GetPlayer()
        {
            return player;
        }

        public void SetWorldPosition(Vector2 worldPosition)
        {
            this.worldPosition = worldPosition;
        }

        public Vector2 GetWorldPosition()
        {
            return worldPosition;
        }
    }
}
