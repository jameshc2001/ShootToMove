using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace ShootToMove
{
    class Button
    {
        public enum ButtonID
        {
            play,
            exit,
            leaderboard,
            options,
            back,
            pauseExit,
            resume,
            fourk,
            fullhd,
            hd,
            fullscreen,
            save
        }

        public enum ButtonState
        {
            waiting,
            hovering,
            hoverClicking,
            hoverClickRelease
        }

        private ButtonID button;
        public ButtonState buttonState;
        private ButtonState prevButtonState; //used to find out when to play a sound and which sound to play
        private Menu menu;
        private Pause pause;
        private Vector2 worldPosition;
        private Texture2D texture;
        private Texture2D checkedTexture;
        private Texture2D uncheckedTexture;
        private AABB boundingBox;
        private SoundEffect selectSnd;

        public Button(ButtonID button, Vector2 worldPosition, Pause pause)
        {
            this.button = button;
            this.worldPosition = worldPosition;
            this.pause = pause;
        }

        public Button(ButtonID button, Vector2 worldPosition, Menu menu)
        {
            this.button = button;
            this.worldPosition = worldPosition;
            this.menu = menu;
        }

        public void LoadContent(ContentManager content)
        {
            if (button == ButtonID.fullscreen) //fullscreen button the only button to have multiple textures
            {
                checkedTexture = content.Load<Texture2D>("Sprites/Buttons/fullcheckedBtn");
                uncheckedTexture = content.Load<Texture2D>("Sprites/Buttons/fullscreenBtn");
                texture = uncheckedTexture;
            }
            else
            {
                string path = "Sprites/Buttons/" + button.ToString() + "Btn";
                texture = content.Load<Texture2D>(path);
            }
            selectSnd = content.Load<SoundEffect>("Sounds/Menus/select");
            boundingBox = new AABB(worldPosition, new Vector2(worldPosition.X + texture.Width, worldPosition.Y + texture.Height));
        }

        public void Draw(SpriteBatch spriteBatch) //buttons are drawn with different shades depending on their state
        {
            switch(buttonState)
            {
                case ButtonState.waiting:
                    spriteBatch.Draw(texture, worldPosition, Color.White);
                    break;
                case ButtonState.hovering:
                    spriteBatch.Draw(texture, worldPosition, Color.DarkGray);
                    if (prevButtonState != buttonState) selectSnd.Play(0.5f, 0, 0);
                    break;
                case ButtonState.hoverClicking:
                    spriteBatch.Draw(texture, worldPosition, Color.Gray);
                    if (prevButtonState != buttonState) selectSnd.Play(0.5f, 0.5f, 0); //plays with different pitch
                    break;
                case ButtonState.hoverClickRelease:
                    spriteBatch.Draw(texture, worldPosition, Color.DarkGray);
                    Debug.WriteLine("Pressed");
                    break;
            }
            prevButtonState = buttonState;
        }

        public void CheckState(GameTime gameTime) //checks what state the button is and acts accordingly
        {
            if (buttonState == ButtonState.hoverClickRelease)
            {
                switch (button)
                {
                    case ButtonID.exit: //exit button on main menu
                        if (menu.menuScreen != Menu.MenuScreen.main) menu.menuScreen = Menu.MenuScreen.main;
                        else menu.game1.Exit();
                        break;
                    case ButtonID.play:
                        menu.level.ResetLevel(Level.LevelID.level1, 0);
                        menu.play = true;
                        break;
                    case ButtonID.pauseExit: //exit button on pause menu is also used for end game screen
                        if (pause != null) //exit button is on pause menu
                        {
                            pause.level.ResetLevel(Level.LevelID.level1, 0);
                            pause.level.menu.play = false;
                        }
                        else //exit button is on end game screen
                        {
                            if (menu.level.score > menu.leaderboard[9].score) //if score is high enough then go to score entry screen
                            {
                                menu.menuScreen = Menu.MenuScreen.scoreEntry;
                            }
                            else //otherwise go back to main menu
                            {
                                menu.menuScreen = Menu.MenuScreen.main;
                            }
                        }
                        break;
                    case ButtonID.resume:
                        pause.level.paused = false;
                        pause.level.camera.menu = false; //camera acts differently in menus
                        break;
                    case ButtonID.options:
                        menu.menuScreen = Menu.MenuScreen.options;
                        break;
                    case ButtonID.fullscreen:
                        if (texture == uncheckedTexture) texture = checkedTexture; //this button is a checkbox so toggle between
                        else texture = uncheckedTexture; //checked and unchecked textures as needed
                        menu.game1.ToggleFullscreen();
                        break;
                    case ButtonID.fourk:
                        menu.game1.ChangeResolution(3840, 2160);
                        break;
                    case ButtonID.fullhd:
                        menu.game1.ChangeResolution(1920, 1080);
                        break;
                    case ButtonID.hd:
                        menu.game1.ChangeResolution(1280, 720);
                        break;
                    case ButtonID.leaderboard:
                        menu.menuScreen = Menu.MenuScreen.leaderboard;
                        break;
                    case ButtonID.save:
                        if (menu.entryText != "") //prevents players from having no name
                        {
                            menu.UpdateLeaderboard(menu.entryText, menu.level.score);
                            menu.menuScreen = Menu.MenuScreen.main;
                        }
                        break;
                }
            }
        }

        public AABB GetBoundingBox()
        {
            return boundingBox;
        }

        public void SetWorldPosition(Vector2 worldPosition)
        {
            this.worldPosition = worldPosition;
            boundingBox = new AABB(worldPosition, new Vector2(worldPosition.X + texture.Width, worldPosition.Y + texture.Height));
        }
    }
}
