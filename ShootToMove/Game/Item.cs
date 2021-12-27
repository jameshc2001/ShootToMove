using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShootToMove
{
    class Item : GameObject
    {
        public enum ItemID
        {
            battery,
            ammo
        }

        private ItemID itemID;
        private Player player;
        private int ScoreToAdd; //score that player gets from picking up the item
        private const float amplitude = 0.5f; //how high the item will float
        private const float speed = 3f; //speed at which it floats
        private const float time = 1f; //how long for the item, if its an ammo box, to respawn
        private Vector2 offset; //to correctly align bounding box with sprite
        private float timer = 0;
        public bool timeout; //true if ammo box picked up. Starts the respawn timer

        public Item(ItemID itemID, Vector2 worldPosition, AABB boundingBox) : base (worldPosition, boundingBox)
        {
            timeout = false;
            this.itemID = itemID;
            switch (itemID)
            {
                case ItemID.battery:
                    this.worldPosition += new Vector2(16, -52); //items sprites are different shapes so must be adjusted accordingly
                    offset = new Vector2(28, 92);
                    ScoreToAdd = 100;
                    break;
                case ItemID.ammo:
                    this.worldPosition += new Vector2(-8, 0);
                    offset = new Vector2(76, 48);
                    ScoreToAdd = 50;
                    break;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            string path = "Sprites/Items/" + itemID.ToString();
            texture = content.Load<Texture2D>(path);
        }

        public override void Update(GameTime gameTime)
        {
            //item floats sinusiodally
            worldPosition.Y += (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * speed) * amplitude;
            boundingBox.UpdateAABB(worldPosition, worldPosition + offset); //bounding box must be updated to align with new position
            if (timeout)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timer > time)
                {
                    timer = 0;
                    timeout = false;
                    if (ScoreToAdd != 0) ScoreToAdd = 0; //only get points for picking up item the first time
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!timeout)
            {
                base.Draw(spriteBatch);
            }
        }

        public int GetScoreToAdd()
        {
            return ScoreToAdd;
        }

        public ItemID GetItemID()
        {
            return itemID;
        }
    }
}
