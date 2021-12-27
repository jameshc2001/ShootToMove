using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShootToMove
{
    class Tank : Enemy
    {
        private Animation move; //this animation plays all the time for the tank, even after death
        private Color color; //the colour that the tank flashes when hit
        private SoundEffect moveSnd; //engine sound, its a ford model T but sounds like a tank
        private SoundEffect dieSnd;
        private Vector2 velocity;
        private Vector2 deathVelocity; //falls through the level after death
        private int prevHp; //used to tell if hp has changed, this info is used to determine whether to flash or not
        private float finalPlayerX; //this variable and the one below are used to determine which direction
        private float finalTankX; //to push the tank in horizontally when it gets destroyed
        private const float time = 3f; //time between firing bullets
        private const float hitTime = 1f; //time it spends flashing after being hit
        private const float flashTime = 0.1f; //how long it spends on a certain colour while flashing
        private float timer; //times reloads
        private float hitTimer; //times how long to flash for
        private float flashTimer; //times how long to spend on a colour when flashing
        private const float speed = 3000;
        private const float gravity = 300; //only used once the tank has been destroyed to make it fall through the level
        private float volume;
        private AABB leftCheck; //checks if the tank is about to drive off the edge to the left
        private AABB rightCheck; //checks if the tank is about to drive off the edge to the right
        private bool rightEdge; //true if tank is at left edge
        private bool leftEdge; //true if tank is at right edge
        private bool flip; //true if sprite needs to be flipped
        private bool hit;


        public Tank(Vector2 worldPosition, Player player) : base (worldPosition, player)
        {
            dead = false;
            stopUpdating = false;
            flip = true;
            rightEdge = false;
            leftEdge = false;
            hit = false;
            hp = 3;
            prevHp = hp;
            score = 500;
            color = Color.White; //drawing image using the colour white has no effect on it, this is a feature of Monogame's spritebatch class
            offset = new Vector2(20, 20);
            boundingBoxSize = new Vector2(108, 48);
            deathVelocity = new Vector2(50, -200);
            boundingBox = new AABB(worldPosition + offset, worldPosition + offset + boundingBoxSize);
            leftCheck = new AABB(worldPosition + new Vector2(0, 68), worldPosition + new Vector2(20, 72));
            rightCheck = new AABB(worldPosition + new Vector2(128, 68), worldPosition + new Vector2(148, 72));
            move = new Animation(Animation.AnimID.tankMove);
            this.worldPosition.Y -= 4;
        }

        public override void LoadContent(ContentManager content)
        {
            move.LoadContent(content);
            moveSnd = content.Load<SoundEffect>("Sounds/Enemies/Tank/tankMoveSnd");
            dieSnd = content.Load<SoundEffect>("Sounds/Enemies/Tank/tankDeath");
            moveSndInstance = moveSnd.CreateInstance();
            moveSndInstance.IsLooped = true;
            texture = move.getFrame(); //set texture to first frame of move
        }

        public override void Update(GameTime gameTime)
        {
            if (prevHp != hp || hp <= 0) hit = true; //determine if tank has been hit (hp set by bullet that collides with it)
            prevHp = hp;

            //depending on distance to camera decide whether or not to continue updating the tank
            distanceToCamera = Vector2.Distance(worldPosition, player.level.camera.GetWorldPosition());
            if (distanceToCamera > 1200 && dead) stopUpdating = true;
            if (hp <= 0) dead = true;

            if (!dead)
            {
                //final x positions with offsets added to account for the sprite size
                finalPlayerX = player.GetWorldPosition().X + 30;
                finalTankX = worldPosition.X + 74;

                if (distanceToCamera < 1200)
                {
                    if (moveSndInstance.State == SoundState.Stopped) moveSndInstance.Play();

                    //change the volume depending on the distance from the camera
                    volume = 3 / distanceToCamera;
                    if (volume > 1) volume = 1;
                    moveSndInstance.Volume = volume;

                    move.Update(gameTime);
                    texture = move.getFrame();

                    //move the tank in the direction it is facing
                    if (!flip)
                    {
                        velocity.X = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        velocity.X = -speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    worldPosition += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    //update AABBs accordingly, check collisions and update the tanks position
                    boundingBox.UpdateAABB(worldPosition + offset, worldPosition + offset + boundingBoxSize);
                    boundingBox.TileCollisions(player.level);
                    worldPosition = boundingBox.min - offset;
                    leftCheck.UpdateAABB(worldPosition + new Vector2(0, 68), worldPosition + new Vector2(20, 72));
                    rightCheck.UpdateAABB(worldPosition + new Vector2(128, 68), worldPosition + new Vector2(148, 72));

                    //set edges to true and then attempt to 'disprove' them
                    leftEdge = true;
                    rightEdge = true;
                    foreach (Tile t in player.level.tileMap)
                    {
                        //if either of the edge AABBs collides with a tile it means that the tank cant be at an edge on that side
                        //the death tile is an exception because it only has a collider so that it can kill the player on collision
                        if (t != null && t.GetTileID() != Tile.TileID.deathFloor)
                        {
                            if (leftCheck.IntersectsWith(t.GetBoundingBox())) leftEdge = false;
                            if (rightCheck.IntersectsWith(t.GetBoundingBox())) rightEdge = false;
                        }
                    }

                    //determine if sprite should be flipped
                    if (boundingBox.rightHit || boundingBox.leftHit || rightEdge || leftEdge)
                    {
                        flip = !flip;
                    }

                    //make it flash
                    if (hit) Flash(gameTime);

                    //fire a bullet if the time has been reached
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timer > time)
                    {
                        timer -= time;

                        Animation bulletExplode = new Animation(Animation.AnimID.bulletExplode);
                        bulletExplode.LoadContent(player.content);

                        Vector2 gunPoint; //position of tip of tanks cannon
                        float gunAngle; //will either be PI or 0 as the tank only ever shoots horizontally
                        if (flip) //make sure to send it in the direction that the tank is facing
                        {
                            gunPoint = worldPosition + new Vector2(-8, 8);
                            gunAngle = (float)Math.PI;
                        }
                        else
                        {
                            gunPoint = worldPosition + new Vector2(140, 8);
                            gunAngle = 0;
                        }

                        player.bullets.Add(new Bullet(gunPoint, player.bulletTexture, new AABB(gunPoint, new Vector2(gunPoint.X + 16, gunPoint.Y + 16)), gunAngle, 400, player, bulletExplode, player.sndEffects[3], true));
                        if (distanceToCamera < 1200) //depending on distance play the sound at certain volume or don't play it at all
                        {
                            float volume = 5 / distanceToCamera;
                            if (volume > 1) volume = 1;
                            player.sndEffects[2].Play(volume, 0, 0);
                        }
                    }

                    //check for collision with player
                    if (boundingBox.IntersectsWith(player.GetBoundingBox()))
                    {
                        if (player.GetBoundingBox().old.max.Y < boundingBox.min.Y)
                        {
                            dead = true;
                            hp = 0;
                            player.SetYVelocity(-240);
                            player.level.score += score;
                            dieSnd.Play();
                        }
                        else
                        {
                            player.isAlive = false;
                        }
                    }
                }
                else
                {
                    if (moveSndInstance.State == SoundState.Playing) moveSndInstance.Stop();
                }
            }
            else //the tank is dead
            {
                //stop playing sound and start flashing
                if (moveSndInstance.State == SoundState.Playing) moveSndInstance.Stop();
                if (hit) Flash(gameTime);

                //determine which direction to push the tank then start moving it
                if (finalPlayerX > finalTankX)
                {
                    if (deathVelocity.X > 0) deathVelocity.X = -deathVelocity.X;
                    deathVelocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    worldPosition += deathVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    deathVelocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    worldPosition += deathVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //drawn to closest integer so that pixels all stay the same size
            Vector2 drawVector = new Vector2(Convert.ToInt32(worldPosition.X), Convert.ToInt32(worldPosition.Y));

            if (!dead)
            {
                if (flip)
                {
                    spriteBatch.Draw(texture, drawVector, null, color, 0.0f, new Vector2(0, 0), 1f, SpriteEffects.FlipHorizontally, 0.0f);
                }
                else
                {
                    spriteBatch.Draw(texture, drawVector, color);
                }
            }
            else
            {
                //make sure to draw tank flipped vertically
                if (flip)
                {
                    //also flipped horizontally so combine the two effects into one effect as done below
                    //SpriteEffects is an enum and has the [Flags] Enum Attribute meaning that enums can be combined using the | character
                    SpriteEffects spriteEffects = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
                    spriteBatch.Draw(texture, drawVector, null, color, 0.0f, new Vector2(0, 0), 1f, spriteEffects, 0.0f);
                }
                else
                {
                    spriteBatch.Draw(texture, drawVector, null, color, 0.0f, new Vector2(0, 0), 1f, SpriteEffects.FlipVertically, 0.0f);
                }
            }
        }

        public void Flash(GameTime gameTime) //handles flashing
        {
            //one cycle of flash timer represents the tank switching to red, waiting flashTime,
            //switching to white and then waiting flashTime again before finally being reset
            flashTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (flashTimer <= flashTime)
            {
                color = Color.Red;
            }
            else if (flashTimer > flashTime && flashTimer <= 2 * flashTime)
            {
                color = Color.White;
            }
            else flashTimer = 0;

            hitTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (hitTimer > hitTime) //stop flashing
            {
                hitTimer = 0;
                hit = false;
                color = Color.White;
            }
        }
    }
}
