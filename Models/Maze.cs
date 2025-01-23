namespace Maze_Game.Models
{
    public class Maze
    {
        public int Energy { get;  set; }
        public (int x, int y) StartPoint { get; private set; }
        public (int x, int y) ExitPoint { get; private set; }
        public int[,] Grid { get; private set; } // Changed to public with private set
        public HashSet<(int x, int y)> DeadEnds { get; private set; } = new();
        public int gainEnergi { get; private set; }
        public int loseEnergy { get; private set; } 

        public Maze(string mazeFilePath)
        {
            LabirentOku(mazeFilePath); // Initialize maze grid from file
            ResetEnergy();
        }

        public void ResetEnergy()
        {
            Energy = 50; // Reset to initial energy value
            gainEnergi = 0; // Reset energy gain counter
            loseEnergy = 0;
        }

        public void Reset()
        {
            ResetEnergy(); // Reset energy
        }

        public void UpdateEnergy(int nodeValue)
        {
            Energy--; // Deduct 1 energy point for movement
            if (nodeValue == 4)
            {
                Energy += 15; 
                gainEnergi += 1;
            } // Energy boost
            if (nodeValue == 5) 
            { 
                Energy -= 10;
                loseEnergy += 1;
            } // Energy penalty
        }

        public bool IsWalkable(int x, int y)
        {
            return x >= 0 && y >= 0
                && x < Grid.GetLength(0)
                && y < Grid.GetLength(1)
                && Grid[x, y] != 0
                && !DeadEnds.Contains((x, y));
        }

        public int GetNodeValue(int x, int y)
        {
            return IsWalkable(x, y) ? Grid[x, y] : 0;
        }

        public void MarkAsDeadEnd(int x, int y)
        {
            DeadEnds.Add((x, y));
        }

        private void LabirentOku(string filePath)
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            int rows = lines.Length;
            int cols = lines[0].Split(',').Length;

            Grid = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                var values = lines[i].Split(',');
                for (int j = 0; j < cols; j++)
                {
                    Grid[i, j] = int.Parse(values[j]);
                    if (Grid[i, j] == 2) StartPoint = (i, j);
                    if (Grid[i, j] == 3) ExitPoint = (i, j);
                }
            }
        }
    }
}
