using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShootToMove
{
    class Animation
    {
        public enum AnimID
        {
            playerRun,
            bulletExplode,
            tankMove,
            landmineExplosion
        }

        private AnimID anim;
        private Texture2D[] frames;
        private Vector2 offset; //animation may be different size to default sprite of object
        private int currentFrame;
        private float interval; //time between frames
        private double timer;
        private bool loop;
        private bool play;

        public Animation(AnimID anim)
        {
            this.anim = anim;
            timer = 0;
            currentFrame = 0;
            play = true;

            switch (anim)
            {
                case AnimID.playerRun:
                    frames = new Texture2D[4];
                    offset = new Vector2(-24, 0);
                    interval = 0.16f;
                    loop = true;
                    break;
                case AnimID.bulletExplode:
                    frames = new Texture2D[5];
                    offset = new Vector2(-32, -32);
                    interval = 0.05f;
                    loop = false;
                    break;
                case AnimID.tankMove:
                    frames = new Texture2D[3];
                    offset = new Vector2(0, 0);
                    interval = 0.1f;
                    loop = true;
                    break;
                case AnimID.landmineExplosion:
                    frames = new Texture2D[11];
                    offset = new Vector2(-128, -332);
                    interval = 0.1f;
                    loop = false;
                    break;
            }
        }

        public void LoadContent(ContentManager content)
        {
            string path = "";
            if (anim == AnimID.playerRun) path = "Animations/Player/" + anim.ToString();
            if (anim == AnimID.bulletExplode) path = "Animations/Effects/" + anim.ToString();
            if (anim == AnimID.tankMove) path = "Animations/Enemies/Tank/" + anim.ToString();
            if (anim == AnimID.landmineExplosion) path = "Animations/Enemies/Landmine/LandmineExplosion/" + anim.ToString();
            int frameNum = 0;
            for (int i = 0; i < frames.Length; i++) //load each frame into the frames array
            {
                frameNum = i + 1;
                frames[i] = content.Load<Texture2D>(path + frameNum);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (play)
            {
                timer += gameTime.ElapsedGameTime.TotalSeconds;
                if (timer > interval) //then go to next frame
                {
                    if (currentFrame == frames.Length - 1 && !loop) play = false;
                    else
                    {
                        timer -= interval;
                        if (currentFrame + 1 == frames.Length) currentFrame = 0; //make sure to go back to start of array if necessary
                        else currentFrame++;
                    }
                }
            }
        }

        public void reset() //if animation is switched away from it must be reset so that when it starts again it starts from frame 1
        {
            timer = 0;
            currentFrame = 0;
        }

        public Texture2D getFrame()
        {
            return frames[currentFrame];
        }

        public int getFrameIndex()
        {
            return currentFrame;
        }

        public Vector2 getOffset()
        {
            return offset;
        }

        public bool getCompleted()
        {
            return !play;
        }
    }
}
