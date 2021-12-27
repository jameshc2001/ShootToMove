namespace ShootToMove
{
    //gives all necessary details about a collision
    class Collision
    {
        public AABB.Direction direction; //direction the collision happened in
        public AABB other; //box collider that the collision happened with

        public Collision(AABB.Direction direction, AABB other)
        {
            this.direction = direction;
            this.other = other;
        }
    }
}
