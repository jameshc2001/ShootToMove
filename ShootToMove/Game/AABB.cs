using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ShootToMove
{
    class AABB
    {
        public enum Direction
        {
            above,
            below,
            left,
            right,
            topLeft,
            topRight,
            bottomLeft,
            bottomRight,
            none
        }

        public AABB old; //holds AABB from last frame
        private List<Collision> Collisions = new List<Collision>();
        public Vector2 min; //top left corner
        public Vector2 max; //bottom right corner
        public bool grounded = false; //true if AABB on ground
        public bool topHit = false; //hit variables true if AABB has hit from that direction
        public bool rightHit = false;
        public bool leftHit = false;
        public bool deathFloor = false; //true if AABB collides with a death foor

        public AABB(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public void UpdateAABB(Vector2 min, Vector2 max)
        {
            old = new AABB(this.min, this.max);
            this.min = min;
            this.max = max;
        }

        public bool IntersectsWith(AABB other)
        {
            if (other == null) return false;

            //separating axis theorem
            if (max.X < other.min.X || min.X > other.max.X) return false; //can we draw a vertical line between them? if so then they are not colliding
            if (max.Y < other.min.Y || min.Y > other.max.Y) return false; //can we draw a horizontal line between them? if so then they are not colliding

            //if execution gets this far then they must be colliding
            return true;
        }

        public void TileCollisions(Level level)
        {
            //loop through all the tiles and cannons. If there is a collision add it to the list of collisions to solve
            //if there is a collision with a death floor then dont solve it but instead make the death floor variable true
            foreach (Tile t in level.tileMap)
            {
                if (t != null && IntersectsWith(t.GetBoundingBox()))
                {
                    if (t.GetTileID() != Tile.TileID.deathFloor) Collisions.Add(new Collision(GetCollisionDirection(t.GetBoundingBox()), t.GetBoundingBox()));
                    else deathFloor = true;
                }
            }
            foreach (Enemy e in level.enemies)
            {
                if (e.GetType() == typeof(Cannon) && IntersectsWith(e.GetBoundingBox()))
                {
                    Collisions.Add(new Collision(GetCollisionDirection(e.GetBoundingBox()), e.GetBoundingBox()));
                }
            }

            //if there is only 1 collision solve it. If there are multiple collisions solve only the ones that are not corner collisions
            //when solving a single collision solve it vertically if it is a corner collision
            topHit = false;
            grounded = false;
            rightHit = false;
            leftHit = false;

            if (Collisions.Count == 1)
            {
                if (Collisions[0].direction != Direction.topLeft && Collisions[0].direction != Direction.topRight && Collisions[0].direction != Direction.bottomLeft && Collisions[0].direction != Direction.bottomRight)
                {
                    SolveCollision(Collisions[0].direction, Collisions[0].other);
                }
                else
                {
                    if (Collisions[0].direction == Direction.topLeft || Collisions[0].direction == Direction.topRight)
                    {
                        SolveCollision(Direction.above, Collisions[0].other);
                    }
                    else
                    {
                        SolveCollision(Direction.below, Collisions[0].other);
                    }
                }

                if (Collisions[0].direction == Direction.below || Collisions[0].direction == Direction.bottomLeft || Collisions[0].direction == Direction.bottomRight) topHit = true;
                if (Collisions[0].direction == Direction.above || Collisions[0].direction == Direction.topLeft || Collisions[0].direction == Direction.topRight) grounded = true;
                if (Collisions[0].direction == Direction.left) leftHit = true;
                if (Collisions[0].direction == Direction.right) rightHit = true;
            }
            else
            {
                foreach (Collision c in Collisions)
                {
                    if (c.direction != Direction.topLeft && c.direction != Direction.topRight && c.direction != Direction.bottomLeft && c.direction != Direction.bottomRight)
                    {
                        SolveCollision(c.direction, c.other);

                        if (c.direction == Direction.below) topHit = true;
                        if (c.direction == Direction.above) grounded = true;
                        if (c.direction == Direction.left) leftHit = true;
                        if (c.direction == Direction.right) rightHit = true;
                    }
                }
            }

            //reset the collision list
            Collisions = new List<Collision>();
        }

        private Direction GetCollisionDirection(AABB other)
        {
            //first step is to determine where the old AABB is in relation to the other AABB
            //there are 8 possibilites. 4 for the corners, above, below, left and right.

            //setting default direction
            Direction direction = Direction.none;

            //first do the 4 main directions
            if (old.max.Y <= other.min.Y)
            {
                direction = Direction.above;
            }
            else if (old.min.Y >= other.max.Y)
            {
                direction = Direction.below;
            }
            else if (old.max.X <= other.min.X)
            {
                direction = Direction.left;
            }
            else if (old.min.X >= other.max.X)
            {
                direction = Direction.right;
            }

            //then do the corners
            if (old.max.X <= other.min.X && old.max.Y <= other.min.Y)
            {
                direction = Direction.topLeft;
            }
            else if (old.min.X >= other.max.X && old.max.Y <= other.min.Y)
            {
                direction = Direction.topRight;
            }
            else if (old.max.X <= other.min.X && old.min.Y >= other.max.Y)
            {
                direction = Direction.bottomLeft;
            }
            else if (old.min.X >= other.max.X && old.min.Y >= other.max.Y)
            {
                direction = Direction.bottomRight;
            }

            return direction;
        }

        private void SolveCollision(Direction direction, AABB other)
        {
            float yDisplacement;
            float xDisplacement;

            //now solve depending on the direciton. The rules are different for each one so there is a lot of code here
            //they all work by finding how much the AABB overlaps with the other AABB then moving it back in the opposite direction
            switch (direction)
            {
                case Direction.none:
                    break;
                case Direction.above:
                    yDisplacement = max.Y - other.min.Y;
                    min.Y -= yDisplacement;
                    max.Y -= yDisplacement;
                    break;
                case Direction.below:
                    yDisplacement = other.max.Y - min.Y;
                    min.Y += yDisplacement;
                    max.Y += yDisplacement;
                    break;
                case Direction.left:
                    xDisplacement = max.X - other.min.X;
                    min.X -= xDisplacement;
                    max.X -= xDisplacement;
                    break;
                case Direction.right:
                    xDisplacement = other.max.X - min.X;
                    min.X += xDisplacement;
                    max.X += xDisplacement;
                    break;
            }
        }

        public override string ToString()
        {
            return "Min: X:" + min.X + " Y:" + min.Y + " Max: X:" + max.X + " Y:" + max.Y;
        }

        public void ForceGrounded(bool grounded)
        {
            this.grounded = grounded;
        }
    }
}
