using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShootToMove
{
    //most objects inherit from this class. This makes sense as most objects have a position
    //in the world, can be collided with and have a texture. Some objects only need some of
    //these things so there are multiple colliders
    class GameObject
    {
        protected Vector2 worldPosition;
        protected Texture2D texture;
        protected AABB boundingBox;

        public GameObject()
        {

        }

        public GameObject(Vector2 worldPosition)
        {
            this.worldPosition = worldPosition;
        }

        public GameObject(Vector2 worldPosition, Texture2D texture)
        {
            this.worldPosition = worldPosition;
            this.texture = texture;
        }

        public GameObject(Vector2 worldPosition, AABB boundingBox)
        {
            this.worldPosition = worldPosition;
            this.boundingBox = boundingBox;
        }

        public GameObject(Vector2 worldPosition, Texture2D texture, AABB boundingBox)
        {
            this.worldPosition = worldPosition;
            this.texture = texture;
            this.boundingBox = boundingBox;
        }

        public virtual void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("");
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, worldPosition, Color.White);
        }

        public Vector2 GetWorldPosition()
        {
            return worldPosition;
        }

        public AABB GetBoundingBox()
        {
            return boundingBox;
        }
    }
}
