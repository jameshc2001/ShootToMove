using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ShootToMove
{
    class Level
    {
        public enum LevelID
        {
            test,
            fallTest,
            runTest,
            level1,
            level2,
            level3
        }

        public LevelID currentLevel;
        private ContentManager Content; //used for loading textures, sounds etc...
        public Menu menu;
        private Pause pause;
        private Player player;
        private Background background;
        public Game1 game1;
        public Camera camera;
        private SpriteFont pixelFont;
        public int score; //the total points that the player has
        private int startScore; //score when the level is first loaded. if player dies and restarts the score is set to this value
        public float gameTimer = 0; //used to calculate score at end of level. Less time higher score
        public Tile[,] tileMap; //level arranged as grid of characters in text file. Eachc character corresponds to a certain tile
        public LevelEnd end; //the end of the level (represented by a sign in the game)
        public List<Item> items = new List<Item>();
        public List<Enemy> enemies = new List<Enemy>();
        public bool paused;
        private KeyboardState prevKState;

        public Level(LevelID currentLevel, Game1 game1, ContentManager Content, int score)
        {
            this.currentLevel = currentLevel;
            this.game1 = game1;
            this.Content = Content;
            this.score = score;
            startScore = score;
            paused = false;
        }

        public void LoadContent(ContentManager content)
        {
            string path = "Content/Levels/" + currentLevel.ToString() + ".txt";
            string levelText = System.IO.File.ReadAllText(path); //levels stored as text files
            GenerateLevel(levelText);
            foreach(Tile t in tileMap)
            {
                if (t != null)
                {
                    t.LoadContent(content);
                }
            }
            foreach (Item i in items)
            {
                i.LoadContent(content);
            }
            foreach (Enemy e in enemies)
            {
                e.LoadContent(content);
                e.SetPlayer(player);
            }
            player.LoadContent(content);
            camera = new Camera(player, game1);
            background = new Background(this, 0.4f);
            background.LoadContent(content);
            pause = new Pause(this);
            pause.LoadContent(content);
            end.LoadContent(content);
            pixelFont = content.Load<SpriteFont>("Fonts/pixelFont");
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState kState = Keyboard.GetState(); //get pressed keys
            if (kState.IsKeyDown(Keys.Escape) && prevKState.IsKeyUp(Keys.Escape)) //without prevKState the pause menu would flicker on and off
            {
                paused = !paused;
                camera.menu = !camera.menu;
            }
            prevKState = kState;

            if (!paused)
            {
                gameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                player.Update(gameTime); //player updated first as input is handled by this class and players will want it to be responsive
                camera.Update(gameTime);
                foreach (Item i in items)
                {
                    if (i != null)
                    {
                        i.Update(gameTime);
                    }
                }
                foreach (Enemy e in enemies)
                {
                    if (!e.stopUpdating) e.Update(gameTime);
                }
                background.Update(gameTime);
            }
            else
            {
                pause.Update(gameTime);
                foreach (Enemy e in enemies) //enemies dont know that game is paused so their sounds keep playing. Have to be muted manually
                {
                    if (e.GetType() == typeof(Tank))
                    {
                        e.KillSound();
                    }
                }
            }
            if (!player.Alive() && kState.IsKeyDown(Keys.Space)) ResetLevel(currentLevel, startScore);
        }

        public void Draw(SpriteBatch spriteBatch) //if x is drawn before y then y, if at same position as x, will be drawn over x
        {
            background.Draw(spriteBatch);
            player.Draw(spriteBatch);
            foreach (Enemy e in enemies)
            {
                if (e.GetType() != typeof(Landmine))
                {
                    if (!e.stopUpdating) e.Draw(spriteBatch);
                }
            }
            //just making sure that landmines are always drawn on top. I want their explosion to consume their victims.
            foreach (Enemy e in enemies)
            {
                if (e.GetType() == typeof(Landmine))
                {
                    if (!e.stopUpdating) e.Draw(spriteBatch);
                }
            }
            foreach (Tile t in tileMap)
            {
                if (t != null)
                {
                    t.Draw(spriteBatch);
                }
            }
            end.Draw(spriteBatch);
            player.DrawBullets(spriteBatch);
            foreach (Item i in items)
            {
                if (i != null)
                {
                    i.Draw(spriteBatch);
                }
            }
            player.DrawCursor(spriteBatch);
            spriteBatch.DrawString(pixelFont, "Score " + score, camera.GetWorldPosition() + new Vector2(-900, -500), Color.White);

            if (!player.Alive())
            {
                spriteBatch.DrawString(pixelFont, "you died", camera.GetWorldPosition() + new Vector2(-100, -30), Color.White);
                spriteBatch.DrawString(pixelFont, "press space to restart", camera.GetWorldPosition() + new Vector2(-340, 30), Color.White);
            }

            if (paused) pause.Draw(spriteBatch); //pause menu drawn on top of rest of game
        }

        public void GenerateLevel(string text)
        {
            //remove new line characters from the text
            string fixedText = text.Replace(Environment.NewLine, string.Empty);

            //find the width and height of the level by comparing the fixed text with the original text
            int width = 0;
            for (int i = 0; i < fixedText.Length; i++)
            {
                if (text[i] == fixedText[i]) //while they are the same increment width
                {
                    width++;
                }
                else
                {
                    i = fixedText.Length;
                }
            }
            int height = fixedText.Length / width;

            //read through the fixed text and use it to create the level
            int textCounter = 0;
            tileMap = new Tile[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Vector2 newWorldPosition = new Vector2(64 * j, 64 * i);

                    switch(fixedText[textCounter])
                    {
                        case 'P':
                            player = new Player(newWorldPosition, new AABB(newWorldPosition, new Vector2(newWorldPosition.X + 64, newWorldPosition.Y + 128)), this);
                            break;
                        case 'f':
                            tileMap[i, j] = new Tile(Tile.TileID.floor, newWorldPosition);
                            break;
                        case 'p':
                            tileMap[i, j] = new Tile(Tile.TileID.platform, newWorldPosition);
                            break;
                        case 'r':
                            tileMap[i, j] = new Tile(Tile.TileID.rightSide, newWorldPosition);
                            break;
                        case 'l':
                            tileMap[i, j] = new Tile(Tile.TileID.leftSide, newWorldPosition);
                            break;
                        case 'b':
                            tileMap[i, j] = new Tile(Tile.TileID.black, newWorldPosition);
                            break;
                        case 'R':
                            tileMap[i, j] = new Tile(Tile.TileID.roof, newWorldPosition);
                            break;
                        case '1':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerBottomLeft, newWorldPosition);
                            break;
                        case '2':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerBottomRight, newWorldPosition);
                            break;
                        case '3':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerTopLeft, newWorldPosition);
                            break;
                        case '4':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerTopRight, newWorldPosition);
                            break;
                        case '5':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerOutsideBottomLeft, newWorldPosition);
                            break;
                        case '6':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerOutsideBottomRight, newWorldPosition);
                            break;
                        case '7':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerOutsideLeftTop, newWorldPosition);
                            break;
                        case '8':
                            tileMap[i, j] = new Tile(Tile.TileID.cornerOutsideRightTop, newWorldPosition);
                            break;
                        case 'D':
                            tileMap[i, j] = new Tile(Tile.TileID.deathFloor, newWorldPosition);
                            break;
                        case '%':
                            items.Add(new Item(Item.ItemID.battery, newWorldPosition, new AABB(newWorldPosition, new Vector2(newWorldPosition.X + 28, newWorldPosition.Y + 92))));
                            break;
                        case 'E':
                            end = new LevelEnd(newWorldPosition + new Vector2(-12, -44), new AABB(newWorldPosition, newWorldPosition + new Vector2(40, 40)));
                            break;
                        case 'A':
                            items.Add(new Item(Item.ItemID.ammo, newWorldPosition, new AABB(newWorldPosition, newWorldPosition + new Vector2(76, 48))));
                            break;
                        case 'T':
                            enemies.Add(new Tank(newWorldPosition, player));
                            break;
                        case 'L':
                            enemies.Add(new Landmine(newWorldPosition, player));
                            break;
                        case 'n':
                            enemies.Add(new Cannon(newWorldPosition, player, Cannon.BulletDirection.left));
                            break;
                        case 'm':
                            enemies.Add(new Cannon(newWorldPosition, player, Cannon.BulletDirection.right));
                            break;
                        case 'N':
                            enemies.Add(new Cannon(newWorldPosition, player, Cannon.BulletDirection.both));
                            break;
                        default:
                            tileMap[i, j] = null;
                            break;
                    }
                    textCounter++;
                }
            }
        }

        public void ResetLevel(LevelID levelID, int score)
        {
            currentLevel = levelID;
            gameTimer = 0;
            this.score = score;
            startScore = score;
            paused = false;
            items.Clear();
            foreach(Enemy e in enemies) //enemies dont know that reset is happening so thier sound must be muted manually
            {
                if (e.GetType() == typeof(Tank))
                {
                    e.KillSound();
                }
            }
            enemies.Clear();
            LoadContent(Content); //now everything has been cleaned up so reload the content for this class
        }

        public Player GetPlayer()
        {
            return player;
        }
    }
}
