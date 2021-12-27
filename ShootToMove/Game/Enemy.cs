using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace ShootToMove
{
    class Enemy : GameObject
    {
        protected float distanceToCamera;
        protected SoundEffectInstance moveSndInstance; //allows greater control over sounds already playing such as tank engine
        protected Player player;
        public int hp; //health points
        public int score; //score player earns for destroying the tank
        protected Vector2 offset; //for bounding box so that it aligns properly with the sprite
        protected Vector2 boundingBoxSize;
        public bool dead;
        public bool stopUpdating; //true if enemy is far from the camera

        public Enemy(Vector2 worldPosition, Player player) : base (worldPosition)
        {
            this.player = player;
            stopUpdating = false;
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        public void KillSound() //when the enemy is killed their sound effects must be stopped separately
        {
            if (moveSndInstance.State == SoundState.Playing) moveSndInstance.Stop();
        }

        public void Kill()
        {
            dead = true;
        }
    }
}
