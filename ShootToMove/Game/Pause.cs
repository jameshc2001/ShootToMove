using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShootToMove
{
    class Pause
    {
        public Level level; //reference to level allows use of camera
        private Texture2D texture; //texture of background of pause menu
        private Texture2D mouseTexture;
        private AABB mouseBoundingBox; //for checking collisions between mouse and buttons
        private Vector2 worldPosition; //changes as player, and camera, move
        private Vector2 offset;
        private Button[] buttons;

        public Pause (Level level)
        {
            this.level = level;
            worldPosition = level.camera.GetWorldPosition();
            buttons = new Button[2];
            buttons[0] = new Button(Button.ButtonID.pauseExit, worldPosition + new Vector2(100, 100), this);
            buttons[1] = new Button(Button.ButtonID.resume, worldPosition + new Vector2(400, 100), this);
        }

        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Sprites/Menus/pauseMenu");
            mouseTexture = content.Load<Texture2D>("Sprites/Player/mouseCursor");
            foreach (Button b in buttons) b.LoadContent(content);

            offset = new Vector2(-texture.Width / 2, -texture.Height / 2); //moves texture to center of screen
        }

        public void Update(GameTime gameTime)
        {
            level.camera.Update(gameTime);
            mouseBoundingBox = new AABB(new Vector2(level.camera.mousePosition.X - 1, level.camera.mousePosition.Y - 1), new Vector2(level.camera.mousePosition.X + 1, level.camera.mousePosition.Y + 1));

            worldPosition = level.camera.GetWorldPosition() + offset;
            buttons[0].SetWorldPosition(worldPosition + new Vector2(176, 260));
            buttons[1].SetWorldPosition(worldPosition + new Vector2(124, 80));

            MouseState mouseState = Mouse.GetState();

            foreach (Button b in buttons)
            {
                if (mouseBoundingBox.IntersectsWith(b.GetBoundingBox()))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed) b.buttonState = Button.ButtonState.hoverClicking;
                    else if (mouseState.LeftButton == ButtonState.Released && b.buttonState == Button.ButtonState.hoverClicking) b.buttonState = Button.ButtonState.hoverClickRelease;
                    else b.buttonState = Button.ButtonState.hovering;
                }
                else b.buttonState = Button.ButtonState.waiting;

                b.CheckState(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, worldPosition, Color.White);
            foreach (Button b in buttons) b.Draw(spriteBatch);
            spriteBatch.Draw(mouseTexture, new Vector2(level.camera.mousePosition.X - 16, level.camera.mousePosition.Y - 16), Color.White);
        }
    }
}
