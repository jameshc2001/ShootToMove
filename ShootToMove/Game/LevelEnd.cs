using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShootToMove
{
    //represented by a signpost in game. Colliding with it sends you to the next level
    class LevelEnd : GameObject
    {
        public LevelEnd(Vector2 worldPosition, AABB boundingBox) : base (worldPosition, boundingBox)
        {

        }

        public override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("Sprites/Checkpoints/levelEnd");
        }
    }
}
