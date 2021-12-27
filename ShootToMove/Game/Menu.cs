using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace ShootToMove
{
    //handles all the menu screens. only once instance of the menu class is needed as it can be switched
    //between which menu it is showing. The only exception to this is the pause menu which has its own class.
    //this class also handles reading and writing to the leaderboard
    class Menu
    {
        public enum MenuScreen
        {
            main,
            leaderboard,
            options,
            end,
            scoreEntry
        }

        public Game1 game1; //reference to game required as some objects access it through the menu
        public Level level; //same reason as above
        public MenuScreen menuScreen;
        private SpriteFont pixelFont;
        public Camera camera;
        private AABB mouseBoundingBox; //used for checking if the mouse is on a button or not
        private KeyboardState prevKState;
        public Button[] mainButtons;
        public Button[] optionsButtons;
        public Button[] leaderboardButtons;
        public Button[] endButtons;
        public Button[] scoreEntryButtons;
        private Background background;
        private Texture2D mouseTexture;
        private Texture2D title;
        private Texture2D optionsMenu;
        private Texture2D leaderboardMenu;
        private Texture2D entryBox;
        public bool play; //used by game class to determine whether the player is in a menu or not (true if in menu mode)
        public string entryText; //text in text box for the score entry screen
        public Score[] leaderboard = new Score[10];

        public Menu(Game1 game1, Level level)
        {
            this.game1 = game1;
            this.level = level;
            entryText = "";
            prevKState = Keyboard.GetState();
            menuScreen = MenuScreen.main; //when initialised the game will be at the main menu
            play = false;
            background = new Background(true);
            camera = new Camera(true, game1);
            mainButtons = new Button[4];
            mainButtons[0] = new Button(Button.ButtonID.play, new Vector2(-580, 92), this);
            mainButtons[1] = new Button(Button.ButtonID.leaderboard, new Vector2(-580, 252), this);
            mainButtons[2] = new Button(Button.ButtonID.options, new Vector2(-580, 412), this);
            mainButtons[3] = new Button(Button.ButtonID.exit, new Vector2(680, 460), this);
            optionsButtons = new Button[5];
            optionsButtons[0] = new Button(Button.ButtonID.fourk, new Vector2(-908, -220), this);
            optionsButtons[1] = new Button(Button.ButtonID.fullhd, new Vector2(-908, -60), this);
            optionsButtons[2] = new Button(Button.ButtonID.hd, new Vector2(-908, 100), this);
            optionsButtons[3] = new Button(Button.ButtonID.fullscreen, new Vector2(-120, 252), this);
            optionsButtons[4] = new Button(Button.ButtonID.exit, new Vector2(680, 460), this);
            leaderboardButtons = new Button[1];
            leaderboardButtons[0] = new Button(Button.ButtonID.exit, new Vector2(680, 460), this);
            endButtons = new Button[1];
            endButtons[0] = new Button(Button.ButtonID.pauseExit, new Vector2(480, 380), this);
            scoreEntryButtons = new Button[1];
            scoreEntryButtons[0] = new Button(Button.ButtonID.save, new Vector2(300, 300), this);
        }

        public void LoadContent(ContentManager content)
        {
            background.LoadContent(content);
            mouseTexture = content.Load<Texture2D>("Sprites/Player/mouseCursor");
            title = content.Load<Texture2D>("Sprites/Menus/Title");
            optionsMenu = content.Load<Texture2D>("Sprites/Menus/optionsMenu");
            leaderboardMenu = content.Load<Texture2D>("Sprites/Menus/leaderboardMenu");
            pixelFont = content.Load<SpriteFont>("Fonts/leaderboardPixelFont");
            entryBox = content.Load<Texture2D>("Sprites/Buttons/entryBox");

            foreach (Button b in mainButtons)
            {
                b.LoadContent(content);
            }

            foreach(Button b in optionsButtons)
            {
                b.LoadContent(content);
            }

            foreach (Button b in leaderboardButtons)
            {
                b.LoadContent(content);
            }

            foreach (Button b in endButtons)
            {
                b.LoadContent(content);
            }

            foreach (Button b in scoreEntryButtons)
            {
                b.LoadContent(content);
            }

            //read the leaderboard file
            StreamReader streamReader = new StreamReader("Content/Leaderboard/leaderboard.txt");
            string text = "";
            string line = "";
            bool name = true; //true if the next thing to be read is a name and not a score
            for (int i = 0; i < leaderboard.Length; i++)
            {
                line = streamReader.ReadLine();
                leaderboard[i] = new Score();
                foreach (char c in line) //read through line, stopping to process the read information if a comma is read
                {
                    if (c == ',')
                    {
                        if (name) leaderboard[i].name = text;
                        else leaderboard[i].score = Convert.ToInt32(text.ToString());
                        name = !name;
                        text = "";
                    }
                    else text += c;
                }
            }
            streamReader.Close();
        }

        public void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
            mouseBoundingBox = new AABB(new Vector2(camera.mousePosition.X - 1, camera.mousePosition.Y - 1), new Vector2(camera.mousePosition.X + 1, camera.mousePosition.Y + 1));

            MouseState mouseState = Mouse.GetState();

            if (menuScreen == MenuScreen.main)
            {
                LoopThroughButtons(mainButtons, mouseState, gameTime);
            }
            else if (menuScreen == MenuScreen.options)
            {
                LoopThroughButtons(optionsButtons, mouseState, gameTime);
            }
            else if (menuScreen == MenuScreen.leaderboard)
            {
                LoopThroughButtons(leaderboardButtons, mouseState, gameTime);
            }
            else if (menuScreen == MenuScreen.end)
            {
                LoopThroughButtons(endButtons, mouseState, gameTime);
            }
            else //must be score entry screen
            {
                LoopThroughButtons(scoreEntryButtons, mouseState, gameTime);

                //no built in text entry system so this is my own. It only allows lower case alphabetical characters
                //and a max length of 14 characters. PrevKState used to make sure characters are only added once per key press.
                KeyboardState kState = Keyboard.GetState();
                if (entryText.Length < 14)
                {
                    if (kState.IsKeyDown(Keys.A) && prevKState.IsKeyUp(Keys.A)) entryText += 'a';
                    else if (kState.IsKeyDown(Keys.B) && prevKState.IsKeyUp(Keys.B)) entryText += 'b';
                    else if (kState.IsKeyDown(Keys.C) && prevKState.IsKeyUp(Keys.C)) entryText += 'c';
                    else if (kState.IsKeyDown(Keys.D) && prevKState.IsKeyUp(Keys.D)) entryText += 'd';
                    else if (kState.IsKeyDown(Keys.E) && prevKState.IsKeyUp(Keys.E)) entryText += 'e';
                    else if (kState.IsKeyDown(Keys.F) && prevKState.IsKeyUp(Keys.F)) entryText += 'f';
                    else if (kState.IsKeyDown(Keys.G) && prevKState.IsKeyUp(Keys.G)) entryText += 'g';
                    else if (kState.IsKeyDown(Keys.H) && prevKState.IsKeyUp(Keys.H)) entryText += 'h';
                    else if (kState.IsKeyDown(Keys.I) && prevKState.IsKeyUp(Keys.I)) entryText += 'i';
                    else if (kState.IsKeyDown(Keys.J) && prevKState.IsKeyUp(Keys.J)) entryText += 'j';
                    else if (kState.IsKeyDown(Keys.K) && prevKState.IsKeyUp(Keys.K)) entryText += 'k';
                    else if (kState.IsKeyDown(Keys.L) && prevKState.IsKeyUp(Keys.L)) entryText += 'l';
                    else if (kState.IsKeyDown(Keys.M) && prevKState.IsKeyUp(Keys.M)) entryText += 'm';
                    else if (kState.IsKeyDown(Keys.N) && prevKState.IsKeyUp(Keys.N)) entryText += 'n';
                    else if (kState.IsKeyDown(Keys.O) && prevKState.IsKeyUp(Keys.O)) entryText += 'o';
                    else if (kState.IsKeyDown(Keys.P) && prevKState.IsKeyUp(Keys.P)) entryText += 'p';
                    else if (kState.IsKeyDown(Keys.Q) && prevKState.IsKeyUp(Keys.Q)) entryText += 'q';
                    else if (kState.IsKeyDown(Keys.R) && prevKState.IsKeyUp(Keys.R)) entryText += 'r';
                    else if (kState.IsKeyDown(Keys.S) && prevKState.IsKeyUp(Keys.S)) entryText += 's';
                    else if (kState.IsKeyDown(Keys.T) && prevKState.IsKeyUp(Keys.T)) entryText += 't';
                    else if (kState.IsKeyDown(Keys.U) && prevKState.IsKeyUp(Keys.U)) entryText += 'u';
                    else if (kState.IsKeyDown(Keys.V) && prevKState.IsKeyUp(Keys.V)) entryText += 'v';
                    else if (kState.IsKeyDown(Keys.W) && prevKState.IsKeyUp(Keys.W)) entryText += 'w';
                    else if (kState.IsKeyDown(Keys.X) && prevKState.IsKeyUp(Keys.X)) entryText += 'x';
                    else if (kState.IsKeyDown(Keys.Y) && prevKState.IsKeyUp(Keys.Y)) entryText += 'y';
                    else if (kState.IsKeyDown(Keys.Z) && prevKState.IsKeyUp(Keys.Z)) entryText += 'z';
                }
                if (kState.IsKeyDown(Keys.Back) && prevKState.IsKeyUp(Keys.Back) && entryText != "") entryText = entryText.Remove(entryText.Length - 1); //adds backspace funtionality
                prevKState = kState;
            }
        }

        public void LoopThroughButtons(Button[] btnArr, MouseState mouseState, GameTime gameTime) //check for button clicks
        {
            foreach (Button b in btnArr)
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

        public void UpdateLeaderboard(string name, int score)
        {
            for (int i = 9; i >= 0; i--)
            {
                if (score > leaderboard[i].score) //then move that score down one
                {
                    if (i != 9) leaderboard[i + 1] = leaderboard[i];
                    if (i == 0) leaderboard[i] = new Score(name, score); //player has the highest score so place them first
                }
                else //correct place for score found
                {
                    if (i != 9) leaderboard[i + 1] = new Score(name, score); //if prevents the players score being placec 11th which doesnt exist in the array
                    i = -1;
                }
            }
            StreamWriter streamWriter = new StreamWriter("Content/Leaderboard/leaderboard.txt"); //now write new leaderboard to file
            string writeText = "";
            for (int i = 0; i < leaderboard.Length; i++)
            {
                writeText = leaderboard[i].name + "," + leaderboard[i].score.ToString() + ","; //preserve formatting
                streamWriter.WriteLine(writeText);
            }
            streamWriter.Close();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);

            if (menuScreen == MenuScreen.main)
            {
                spriteBatch.Draw(title, new Vector2(-960, -540), Color.White);
                foreach (Button b in mainButtons)
                {
                    b.Draw(spriteBatch);
                }
            }
            else if (menuScreen == MenuScreen.options)
            {
                spriteBatch.Draw(optionsMenu, new Vector2(-960, -540), Color.White);
                foreach (Button b in optionsButtons)
                {
                    b.Draw(spriteBatch);
                }
            }
            else if (menuScreen == MenuScreen.leaderboard)
            {
                spriteBatch.Draw(leaderboardMenu, new Vector2(-960, -540), Color.White);
                foreach (Button b in leaderboardButtons)
                {
                    b.Draw(spriteBatch);
                }
                for (int i = 0; i < leaderboard.Length; i++)
                {
                    spriteBatch.DrawString(pixelFont, leaderboard[i].name, new Vector2(-900, (i * 84) - 320), Color.White);
                    spriteBatch.DrawString(pixelFont, leaderboard[i].score.ToString(), new Vector2(100, (i * 84) - 320), Color.White);
                }
            }
            else if (menuScreen == MenuScreen.end)
            {
                foreach (Button b in endButtons)
                {
                    b.Draw(spriteBatch);
                }
                spriteBatch.DrawString(pixelFont, "Congratulations your final", new Vector2(-900, -480), Color.White);
                spriteBatch.DrawString(pixelFont, "score is " + level.score.ToString(), new Vector2(-900, -400), Color.White);
                if (level.score > leaderboard[9].score)
                {
                    spriteBatch.DrawString(pixelFont, "Your score is high enough to", new Vector2(-900, -320), Color.White);
                    spriteBatch.DrawString(pixelFont, "be placed on the leaderboard", new Vector2(-900, -240), Color.White);
                    spriteBatch.DrawString(pixelFont, "Press the exit button to submit it", new Vector2(-900, -160), Color.White);
                }
                else
                {
                    spriteBatch.DrawString(pixelFont, "Your score is not high enough to", new Vector2(-900, -320), Color.White);
                    spriteBatch.DrawString(pixelFont, "be placed on the leaderboard", new Vector2(-900, -240), Color.White);
                    spriteBatch.DrawString(pixelFont, "Press the exit button to try again", new Vector2(-900, -160), Color.White);
                }
            }
            else //must be score entry screen
            {
                foreach (Button b in scoreEntryButtons)
                {
                    b.Draw(spriteBatch);
                }
                spriteBatch.Draw(entryBox, new Vector2(-700, 300), Color.White);
                spriteBatch.DrawString(pixelFont, "Please enter your name", new Vector2(-900, -480), Color.White);
                spriteBatch.DrawString(pixelFont, "and then click save", new Vector2(-900, -400), Color.White);
                spriteBatch.DrawString(pixelFont, entryText, new Vector2(-680, 316), Color.White);
            }

            //mouse is always drawn no matter the menu
            spriteBatch.Draw(mouseTexture, new Vector2(camera.mousePosition.X - 16, camera.mousePosition.Y - 16), Color.White);
        }
    }
}
