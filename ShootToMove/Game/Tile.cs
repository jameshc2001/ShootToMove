using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShootToMove
{
    class Tile : GameObject
    {
        public enum TileID //all the possible tiles
        {
            black,
            cornerBottomLeft,
            cornerBottomRight,
            cornerOutsideBottomLeft,
            cornerOutsideBottomRight,
            cornerOutsideLeftTop,
            cornerOutsideRightTop,
            cornerTopLeft,
            cornerTopRight,
            floor,
            leftSide,
            platform,
            rightSide,
            roof,
            deathFloor
        }

        private readonly TileID tileType; //tile cannot be changed during runtime

        public Tile(TileID tileType, Vector2 worldPosition)
        {
            this.tileType = tileType;
            this.worldPosition = worldPosition;

            //make bounding box
            if (tileType != TileID.black)
            {
                boundingBox = new AABB(worldPosition, new Vector2(worldPosition.X + 64, worldPosition.Y + 64)); //tiles have area of 64x64 pixels
            }
            else
            {
                boundingBox = null;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            string path = "Sprites/Tiles/" + tileType.ToString();
            texture = content.Load<Texture2D>(path);
        }

        public override string ToString() //use for debugging purposes
        {
            return "Tile of type " + tileType.ToString() + " with coordinates X:" + worldPosition.X + " Y:" + worldPosition.Y;
        }

        public TileID GetTileID()
        {
            return tileType;
        }
    }
}
