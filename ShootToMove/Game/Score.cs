namespace ShootToMove
{
    class Score //mean class has an array of scores that acts as a leaderboard
    {
        public string name;
        public int score;

        public Score() //allows instantiation of an empty score
        {

        }

        public Score(string name, int score)
        {
            this.name = name;
            this.score = score;
        }

        public override string ToString() //for debugging purposes
        {
            return "Name: " + name + "  Score: " + score.ToString();
        }
    }
}
