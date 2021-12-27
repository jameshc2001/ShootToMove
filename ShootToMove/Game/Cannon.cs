using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShootToMove
{
    class Cannon : Enemy
    {
        public enum BulletDirection
        {
            left,
            right,
            both
        }

        private BulletDirection bulletDirection; //turrets can shoot left, right or both
        private const float time = 2f; //time between shots
        private float timer;
        private Vector2 leftGunPoint;
        private Vector2 rightGunPoint;

        public Cannon(Vector2 worldPosition, Player player, BulletDirection bulletDirection) : base(worldPosition, player)
        {
            timer = 0;
            this.bulletDirection = bulletDirection;
            this.worldPosition += new Vector2(0, -64);
            leftGunPoint = this.worldPosition + new Vector2(-20, 24); //these two vectors account for the size of the sprite
            rightGunPoint = this.worldPosition + new Vector2(84, 24);
            boundingBox = new AABB(this.worldPosition + new Vector2(0, 8), this.worldPosition + new Vector2(64, 128));
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Sprites/Enemies/Cannon/cannon");
        }

        public override void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > time)
            {
                timer -= time;
                Animation bulletExplode;

                distanceToCamera = Vector2.Distance(worldPosition, player.level.camera.GetWorldPosition());

                if (distanceToCamera < 1200)
                {
                    switch (bulletDirection)
                    {
                        //to create a new bullet it also needs an explode animation
                        //all bullets owned by player so access to player class needed
                        case BulletDirection.left:
                            bulletExplode = new Animation(Animation.AnimID.bulletExplode);
                            bulletExplode.LoadContent(player.content);
                            player.bullets.Add(new Bullet(leftGunPoint, player.bulletTexture, new AABB(leftGunPoint, new Vector2(leftGunPoint.X + 16, leftGunPoint.Y + 16)), (float)Math.PI, 300, player, bulletExplode, player.sndEffects[3], true));
                            PlayBulletSound();
                            break;
                        case BulletDirection.right:
                            bulletExplode = new Animation(Animation.AnimID.bulletExplode);
                            bulletExplode.LoadContent(player.content);
                            player.bullets.Add(new Bullet(rightGunPoint, player.bulletTexture, new AABB(rightGunPoint, new Vector2(rightGunPoint.X + 16, rightGunPoint.Y + 16)), 0, 300, player, bulletExplode, player.sndEffects[3], true));
                            PlayBulletSound();
                            break;
                        case BulletDirection.both:
                            bulletExplode = new Animation(Animation.AnimID.bulletExplode);
                            bulletExplode.LoadContent(player.content);
                            player.bullets.Add(new Bullet(leftGunPoint, player.bulletTexture, new AABB(leftGunPoint, new Vector2(leftGunPoint.X + 16, leftGunPoint.Y + 16)), (float)Math.PI, 300, player, bulletExplode, player.sndEffects[3], true));
                            bulletExplode = new Animation(Animation.AnimID.bulletExplode);
                            bulletExplode.LoadContent(player.content);
                            player.bullets.Add(new Bullet(rightGunPoint, player.bulletTexture, new AABB(rightGunPoint, new Vector2(rightGunPoint.X + 16, rightGunPoint.Y + 16)), 0, 300, player, bulletExplode, player.sndEffects[3], true));
                            PlayBulletSound();
                            break;
                    }
                }
            }
        }

        public void PlayBulletSound()
        {
            if (distanceToCamera < 1200) //only play sound when within range of camera
            {
                float volume = 100 / distanceToCamera;
                if (volume > 1) volume = 1;
                player.sndEffects[2].Play(volume, 0, 0);
            }
        }
    }
}
