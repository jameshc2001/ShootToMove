using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShootToMove
{
    class Landmine : Enemy
    {
        private Animation explode;
        private SoundEffect explosionSnd;
        private bool justDied;

        public Landmine(Vector2 worldPosition, Player player) : base(worldPosition, player)
        {
            dead = false;
            justDied = true; //this variable remains true until one frame after death. Allows certain operations to be done only once
            stopUpdating = false;
            hp = 1;
            score = 100;
            this.worldPosition += new Vector2(4,52); //position adjusted so that landmine rests on floor
            boundingBox = new AABB(this.worldPosition, this.worldPosition + new Vector2(56, 12)); //boudning box adjusted
        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Sprites/Enemies/Landmine/landmine");
            explosionSnd = content.Load<SoundEffect>("Sounds/Enemies/Landmine/landmineExplosion");
            explode = new Animation(Animation.AnimID.landmineExplosion);
            explode.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (!dead)
            {
                if (boundingBox.IntersectsWith(player.GetBoundingBox()))
                {
                    player.isAlive = false; //player dies imediately if they walk on landmine
                    player.SetYVelocity(-600); //bounce the player up from the explosion
                    dead = true; //the landmine also unfortunately dies, taking the player with it like a true hero
                    hp = 0;
                }
                foreach (Enemy e in player.level.enemies)
                {
                    if (e.GetType() == typeof(Tank)) //this was harder to do that I thought
                    {
                        if (boundingBox.IntersectsWith(e.GetBoundingBox())) //it also kills enemies
                        {
                            e.hp = 0;
                            e.dead = true;
                            dead = true;
                            hp = 0;
                        }
                    }
                }
            }
            else
            {
                if (justDied) //do the things inside this if statement only once
                {
                    explosionSnd.Play();
                    worldPosition += explode.getOffset(); //explosion animation larger than landmine sprite. Must be adjusted
                    justDied = false;
                }
                explode.Update(gameTime); //update the explode animation
                texture = explode.getFrame(); //get latest frame from explode animation
                if (explode.getCompleted()) stopUpdating = true; //once completed prevent landmine from being updated as its a waste of time
            }
        }
    }
}
