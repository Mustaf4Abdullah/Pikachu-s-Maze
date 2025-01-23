namespace Maze_Game.Models
{
    public class MazeViewModel
    {
        public int[,] Grid { get; set; }
        public List<string> Path { get; set; }
        public int Energy { get; set; } // Add this property

    }

}
