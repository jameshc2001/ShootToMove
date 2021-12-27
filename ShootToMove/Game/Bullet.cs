using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShootToMove
{
    class Bullet : GameObject
    {
        private Animation explode;
        private Player player; //needs reference to player as all bullets are owned by the player and level can be accessed through player
        private Vector2 velocity;
        private SoundEffect explodeSnd;
        public bool impact; //true if bullet hits something
        public bool dead; //only true after explode animation has ended
        public bool enemy; //true if bullet was fired by enemy
        private bool corrected; //becomes true after position of bulleth as been changed to accomodate the explode animation
        private float distanceToCamera;

        public Bullet(Vector2 worldPosition, Texture2D texture, AABB boundingBox, float angle, float force, Player player, Animation explode, SoundEffect explodeSnd, bool enemy) : base (worldPosition, texture, boundingBox)
        {
            velocity.Y += force * (float)Math.Sin(angle); //split angle and velocity up into its
            velocity.X += force * (float)Math.Cos(angle); //horizontal and vertical components
            this.player = player;
            this.explode = explode;
            this.explodeSnd = explodeSnd;
            this.enemy = enemy;
            impact = false;
            dead = false;
            corrected = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!impact)
            {
                worldPosition += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds; //update position then bounding box
                boundingBox.UpdateAABB(worldPosition, new Vector2(worldPosition.X + 16, worldPosition.Y + 16));

                foreach (Tile t in player.level.tileMap) //check for collisions with each tile excluding death floors (tiles that kill player)
                {
                    if (t != null && boundingBox.IntersectsWith(t.GetBoundingBox()) && t.GetTileID() != Tile.TileID.deathFloor) impact = true;
                }
                foreach (Enemy e in player.level.enemies) //check for collisions with enemies that are alive
                {
                    if (!e.dead && !enemy && boundingBox.IntersectsWith(e.GetBoundingBox())) //if player bullet
                    {
                        e.hp--;
                        if (e.GetType() == typeof(Landmine))
                        {
                            e.dead = true; //landmines blow up after one hit
                            player.level.score += e.score;
                        }
                        impact = true;
                    }
                    if (!e.dead && enemy && boundingBox.IntersectsWith(e.GetBoundingBox())) //if enemy bullet
                    {
                        impact = true;
                        if (e.GetType() != typeof(Cannon)) //only take hp from landmines and tanks
                        {
                            e.hp--;
                        }
                    }
                }
                if (enemy && boundingBox.IntersectsWith(player.GetBoundingBox())) //if enemy bullet hits player
                {
                    player.isAlive = false; //player dies in one hit
                    impact = true;
                }
            }
            else
            {
                if (!corrected)
                {
                    distanceToCamera = Vector2.Distance(worldPosition, player.level.camera.GetWorldPosition());
                    worldPosition += explode.getOffset(); //applies animation offset so that correctly aligns with where the bullet hit
                    corrected = true;
                    if (distanceToCamera < 1200)
                    {
                        float volume = 100 / distanceToCamera;
                        if (volume > 1) volume = 1;
                        explodeSnd.Play(volume, 0, 0);
                    }
                }

                explode.Update(gameTime); //update the animation
                texture = explode.getFrame(); //update the texture with the current frame from the animation

                if (explode.getCompleted()) dead = true; //once anim is completed bullet is marked as dead and will later be deleted
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!enemy) base.Draw(spriteBatch);
            else
            {
                spriteBatch.Draw(texture, worldPosition, Color.Orange); //draw enemy bullets a different colour
            }
        }
    }
}
