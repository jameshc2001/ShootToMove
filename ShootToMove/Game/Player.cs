using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace ShootToMove
{
    class Player : GameObject
    {
        public Level level;
        public ContentManager content; //used to load content for bullets
        public bool isAlive;
        private bool flip; //true if sprite needs to be flipped horizontally
        private bool oldTopHit; //used to decide if player has just hit their head on collider above them
        private bool canReload;
        private bool playDeathSound; //allows death sound to be played but only once
        private bool firstFrame;
        private bool canJump;
        private int ammo = 1;
        private int animFrameCounter = 0; //certain frames have sounds associated with them. this variable keeps track of the frame index
        private int previousAnim; //index of previous animation
        private int currentAnim; //index of current animation
        private const float reloadTime = 1;
        private const float gravity = 600;
        private const float jumpForce = 420;
        private const float maxFallSpeed = 600;
        private const float maxSpeed = 480;
        private const float maxSpeedInAir = 120; //this is the max speed the player can add once in the air
        private const float accelerationInAir = 240; //how quickly the players input affects movement in air
        private const float acceleration = 900; //same as above but on ground
        private const float xResistance = 360; //without input the player's x speed in air is decreased by this amount
        private const float gunForce = 480; //force from shooting gun
        private const float bulletSpeed = 600;
        private float xVelocityInAir = 0;
        private float gunAngle = 0;
        private float reloadTimer = 0;
        private Vector2 drawVector; //draw vector different to worldPosition as must be drawn to nearest integer (pixel)
        private Vector2 velocity;
        private Vector2 gunPosition;
        private Vector2 textureOffset; //so texture is aligned with collider
        private Texture2D idleTexture;
        private Texture2D jumpTexture;
        private Texture2D mouseTexture; //mouse cursor
        private Texture2D gunArmTexture;
        public Texture2D bulletTexture;
        public List<Bullet> bullets = new List<Bullet>(); //player owns all the bullets, including enemy bullets
        private Animation[] anims = new Animation[1]; //i planned to have multiple animations, turns out animations are difficult
        public SoundEffect[] sndEffects = new SoundEffect[5];
        private KeyboardState kState;
        private MouseState mState;
        private MouseState prevMState;

        public Player(Vector2 worldPosition, AABB boundingBox, Level level) : base (worldPosition, boundingBox)
        {
            isAlive = true;
            playDeathSound = true; //becomes values as soon as its played
            flip = false;
            oldTopHit = false;
            this.level = level;
            anims[0] = new Animation(Animation.AnimID.playerRun);
            canReload = true;
            firstFrame = true;
            canJump = false; //player cant jump on first frame so if they restart (press space after dying) they dont just as soon as they respawn
        }

        public override void LoadContent(ContentManager content)
        {
            this.content = content;

            idleTexture = content.Load<Texture2D>("Sprites/Player/player");
            jumpTexture = content.Load<Texture2D>("Sprites/Player/playerJump");
            mouseTexture = content.Load<Texture2D>("Sprites/Player/mouseCursor");
            gunArmTexture = content.Load<Texture2D>("Sprites/Player/gunArm");
            bulletTexture = content.Load<Texture2D>("Sprites/Projectiles/bullet");

            sndEffects[0] = content.Load<SoundEffect>("Sounds/Player/Run");
            sndEffects[1] = content.Load<SoundEffect>("Sounds/Items/BatterySnd");
            sndEffects[2] = content.Load<SoundEffect>("Sounds/Player/shoot");
            sndEffects[3] = content.Load<SoundEffect>("Sounds/Effects/bulletExplode");
            sndEffects[4] = content.Load<SoundEffect>("Sounds/Player/playerDeath");

            foreach (Animation a in anims) a.LoadContent(content);

            texture = idleTexture;
        }

        public override void Update(GameTime gameTime)
        {
            //if its the first frame fix the players position so they dont start by falling. a lot of ugly code to fix one bug ;(
            if (firstFrame)
            {
                firstFrame = false;
                worldPosition.Y -= 44;
                boundingBox.UpdateAABB(new Vector2(worldPosition.X + 12, worldPosition.Y + 4 - 20), new Vector2(worldPosition.X + 48, worldPosition.Y + 128 - 20));
                boundingBox.UpdateAABB(new Vector2(worldPosition.X + 12, worldPosition.Y + 4), new Vector2(worldPosition.X + 48, worldPosition.Y + 128));
                boundingBox.TileCollisions(level);
                worldPosition = new Vector2(boundingBox.min.X - 12, boundingBox.min.Y - 4);
            }

            //get keyboard state and mouse state and set previous anim
            kState = Keyboard.GetState();
            prevMState = mState;
            mState = Mouse.GetState();
            previousAnim = currentAnim;

            //reset xVelocityInAir if the player dies.
            if (!isAlive) xVelocityInAir = 0;

            //prevent the player from jumping after hitting space to restart;
            if (!canJump && kState.IsKeyUp(Keys.Space)) canJump = true;

            //check if the player died last frame
            if (boundingBox.deathFloor) isAlive = false;

            //reload
            if (canReload && boundingBox.grounded) ammo = 1;
            if (!canReload)
            {
                reloadTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (reloadTimer > reloadTime)
                {
                    canReload = true;
                    reloadTimer = 0;
                }
            }

            //if player hits a wall and tries to keep moving through it reset the x velocity
            if (boundingBox.leftHit == true && velocity.X + xVelocityInAir > 0)
            {
                velocity.X = 0;
                xVelocityInAir = 0;
            }
            if (boundingBox.rightHit == true && velocity.X + xVelocityInAir < 0)
            {
                velocity.X = 0;
                xVelocityInAir = 0;
            }

            //apply gravity and limit y velocity and reset air velocity and ammo
            if (!boundingBox.grounded)
            {
                velocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (isAlive)
            {
                velocity.Y = gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                xVelocityInAir = 0;
            }
            if (velocity.Y > maxFallSpeed) velocity.Y = maxFallSpeed;

            //bounce player back if they just hit their head
            if (!oldTopHit && boundingBox.topHit)
            {
                velocity.Y = -velocity.Y;
                sndEffects[0].Play(0.25f, 0.25f, 0);
            }
            oldTopHit = boundingBox.topHit;

            //if player is at apex of jump them push them down faster
            if (Convert.ToInt32(velocity.Y) == 0 && !boundingBox.grounded) velocity.Y += 5f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //handle input. the way input is handled changes depending on if the player is grounded or not
            if (kState.IsKeyDown(Keys.A) && isAlive)
            {
                if (boundingBox.grounded)
                {
                    velocity.X -= acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Math.Abs(velocity.X) > maxSpeed) velocity.X = -maxSpeed;
                }
                else
                {
                    if (xVelocityInAir > 0) xVelocityInAir = 0;
                    xVelocityInAir -= accelerationInAir * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Math.Abs(xVelocityInAir) > maxSpeedInAir) xVelocityInAir = -maxSpeedInAir;
                }
                flip = true;
            }
            if (kState.IsKeyDown(Keys.D) && isAlive)
            {
                if (boundingBox.grounded)
                {
                    velocity.X += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Math.Abs(velocity.X) > maxSpeed) velocity.X = maxSpeed;
                }
                else
                {
                    if (xVelocityInAir < 0) xVelocityInAir = 0;
                    xVelocityInAir += accelerationInAir * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Math.Abs(xVelocityInAir) > maxSpeedInAir) xVelocityInAir = maxSpeedInAir;
                }
                flip = false;
            }
            if (boundingBox.grounded && velocity.X < 0 && kState.IsKeyUp(Keys.A))
            {
                velocity.X = 0;
            }
            if (boundingBox.grounded && velocity.X > 0 && kState.IsKeyUp(Keys.D))
            {
                velocity.X = 0;
            }
            if (xVelocityInAir + velocity.X < 0 && kState.IsKeyUp(Keys.A))
            {
                xVelocityInAir = 0;
            }
            if (xVelocityInAir + velocity.X > 0 && kState.IsKeyUp(Keys.D))
            {
                xVelocityInAir = 0;
            }
            if (kState.IsKeyDown(Keys.Space) && boundingBox.grounded && canJump)
            {
                velocity.Y = -jumpForce;
            }
            if (mState.LeftButton == ButtonState.Pressed && ammo != 0 && isAlive && prevMState.LeftButton == ButtonState.Released)
            {
                ammo = 0;
                xVelocityInAir = 0;
                canReload = false;

                //get position at which to spawn the bullet. then spawn the bullet
                Vector2 gunPoint = new Vector2(worldPosition.X + 16, worldPosition.Y + 40);
                gunPoint.Y += 64 * (float)Math.Sin(gunAngle);
                gunPoint.X += 64 * (float)Math.Cos(gunAngle);
                Animation bulletExplode = new Animation(Animation.AnimID.bulletExplode);
                bulletExplode.LoadContent(content);
                bullets.Add(new Bullet(gunPoint, bulletTexture, new AABB(gunPoint, new Vector2(gunPoint.X + 16, gunPoint.Y + 16)), gunAngle, bulletSpeed, this, bulletExplode, sndEffects[3], false));

                //get direction of force
                gunAngle -= (float)Math.PI;
                velocity.Y += gunForce * (float)Math.Sin(gunAngle);
                velocity.X += gunForce * (float)Math.Cos(gunAngle);

                if (velocity.Y > maxFallSpeed) velocity.Y = maxFallSpeed;

                sndEffects[2].Play();
            }

            //add x resistance
            if (!boundingBox.grounded && velocity.Y > 0)
            {
                if (velocity.X > 0)
                {
                    velocity.X -= xResistance * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (velocity.X < 0) velocity.X = 0;
                }
                if (velocity.X < 0)
                {
                    velocity.X += xResistance * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (velocity.X > 0) velocity.X = 0;
                }
            }

            //apply y velocity and x velocity
            worldPosition.Y += velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;
            worldPosition.X += (velocity.X + xVelocityInAir) * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //update bounding box
            if (boundingBox.grounded) boundingBox.UpdateAABB(new Vector2(worldPosition.X + 12, worldPosition.Y + 4), new Vector2(worldPosition.X + 48, worldPosition.Y + 128));
            else boundingBox.UpdateAABB(new Vector2(worldPosition.X + 12, worldPosition.Y + 4), new Vector2(worldPosition.X + 48, worldPosition.Y + 108));

            //perform collisions with tiles
            if (isAlive) boundingBox.TileCollisions(level);
            else boundingBox.ForceGrounded(false);

            //update world position after collisions and fix it if the player just landed and update draw vector
            worldPosition = new Vector2(boundingBox.min.X - 12, boundingBox.min.Y - 4);
            if (boundingBox.grounded && texture == jumpTexture)
            {
                worldPosition.Y -= 20;
                velocity.X = 0;
                sndEffects[0].Play(0.25f, 0.25f, 0);
            }
            drawVector = new Vector2(Convert.ToInt32(worldPosition.X), Convert.ToInt32(worldPosition.Y));

            //calculate angle between gun and mouse
            gunPosition = new Vector2(drawVector.X + 24, drawVector.Y + 48);
            if (flip) gunPosition.X += 12;
            gunAngle = (float)Math.Atan(Convert.ToDouble((level.camera.mousePosition.Y - gunPosition.Y) / (level.camera.mousePosition.X - gunPosition.X)));
            if (level.camera.mousePosition.X < gunPosition.X) gunAngle += (float)Math.PI;

            //update bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                //not using an if else as running the update can change the value of dead
                if (!bullets[i].dead) bullets[i].Update(gameTime);
                if (bullets[i].dead) bullets.RemoveAt(i);
            }

            if (isAlive)
            {
                //check for collisions with items and add score as necessary
                for (int i = 0; i < level.items.Count; i++)
                {
                    if (level.items[i] != null && !level.items[i].timeout)
                    {
                        if (boundingBox.IntersectsWith(level.items[i].GetBoundingBox()))
                        {
                            level.score += level.items[i].GetScoreToAdd();
                            if (level.items[i].GetItemID() == Item.ItemID.ammo)
                            {
                                ammo = 1;
                                reloadTimer = 0;
                                level.items[i].timeout = true;
                            }
                            else
                            {
                                level.items[i] = null;
                            }
                            sndEffects[1].Play();
                        }
                    }
                }

                //check for collision with level end and load correct next level
                if (boundingBox.IntersectsWith(level.end.GetBoundingBox()))
                {
                    level.score += Convert.ToInt32((1 / level.gameTimer) * 100000);

                    switch(level.currentLevel)
                    {
                        case Level.LevelID.level1:
                            level.ResetLevel(Level.LevelID.level2, level.score);
                            break;
                        case Level.LevelID.level2:
                            level.ResetLevel(Level.LevelID.level3, level.score);
                            break;
                        case Level.LevelID.level3:
                            foreach(Enemy e in level.enemies)
                            {
                                if (e.GetType() == typeof(Tank))
                                {
                                    e.KillSound();
                                    e.Kill();
                                }
                            }
                            level.game1.FinalLevelCompleted(); //tells game to switch to menu mode (menu.play = false)
                            level.menu.menuScreen = Menu.MenuScreen.end;
                            break;
                    }
                }
            }
            else if (playDeathSound)
            {
                sndEffects[4].Play();
                playDeathSound = false;
            }

            //set correct animation and reset the old animation for later
            if (Math.Abs(velocity.X) > 0 && boundingBox.grounded)
            {
                currentAnim = 0;
                anims[currentAnim].Update(gameTime);
                texture = anims[currentAnim].getFrame();
                textureOffset = anims[currentAnim].getOffset();

                //play sound
                if (anims[currentAnim].getFrameIndex() == animFrameCounter)
                {
                    sndEffects[0].Play(0.25f, 0.25f, 0);
                    if (anims[currentAnim].getFrameIndex() == 0) animFrameCounter = 2;
                    if (anims[currentAnim].getFrameIndex() == 2) animFrameCounter = 0;
                }
            }
            else animFrameCounter = 0;
            if ((boundingBox.grounded && Math.Floor(velocity.X) == 0) || (boundingBox.grounded && (boundingBox.leftHit || boundingBox.rightHit)))
            {
                //player is on floor and standing still
                texture = idleTexture;
                textureOffset = new Vector2(0, 0);
                anims[currentAnim].reset();
            }
            if (!boundingBox.grounded)
            {
                texture = jumpTexture;
                textureOffset = new Vector2(0, -16);
                anims[currentAnim].reset();
            }
            if (previousAnim != currentAnim) //if animations have been switched then the old one needs to be set back to its first frame
            {
                anims[currentAnim].reset();
                previousAnim = currentAnim;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //draw player
            if (flip)
            {
                spriteBatch.Draw(texture, drawVector + textureOffset, null, Color.White, 0.0f, new Vector2(0, 0), 1f, SpriteEffects.FlipHorizontally, 0.0f);
            }
            else
            {
                //base.Draw(spriteBatch);
                spriteBatch.Draw(texture, drawVector + textureOffset, Color.White);
            }

            //draw gun arm
            spriteBatch.Draw(gunArmTexture, gunPosition, null, Color.White, gunAngle, new Vector2(8, 8), 1.0f, SpriteEffects.None, 1.0f);
        }

        public void DrawBullets(SpriteBatch spriteBatch)
        {
            foreach (Bullet b in bullets)
            {
                b.Draw(spriteBatch);
            }
        }

        public void DrawCursor(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mouseTexture, new Vector2(level.camera.mousePosition.X - 16, level.camera.mousePosition.Y - 16), Color.White);
        }

        public bool Alive()
        {
            return isAlive;
        }

        public void SetYVelocity(float yVelocity) //used by various classes to make player bounce up e.g. in response to landmine explosion
        {
            velocity.Y = yVelocity;
        }
    }
}
