using Microsoft.AspNetCore.Mvc;
using Maze_Game.Models;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace MazeGame.Controllers
{
    public class MazeController : Controller
    {
        private readonly Maze _maze;
        private const int MaxResets = 10;
        private int  resetCount = 0;


        public MazeController(IWebHostEnvironment webHostEnvironment)
        {
            string mazeFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "App_Data/Maze.txt");
            _maze = new Maze(mazeFilePath);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Solve()
        {
            var result = SolveMaze();
            return View("Index", result);
        }

        public IActionResult Simulate()
        {
            var path = SolveMaze();
            return View("Simulate", new MazeViewModel { Grid = _maze.Grid, Path = path, Energy = _maze.Energy });
        }
        [HttpPost]
        public IActionResult SaveSimulationResults([FromBody] SimulationResult result)
        {
            if (result == null)
            {
                return BadRequest("Invalid simulation data received.");
            }

            // Prepare the data to write to the file
            List<string> output = new List<string>
    {
        $"Simulation Results:",
        
        $"Repeat Count: {result.RepeatCount}",
        $"Shortest Path Length: {result.ShortestPath.Count}",
        $"Shortest Path: {string.Join(" -> ", result.ShortestPath)}",
        $"Teprikler" + _maze.gainEnergi + " Adet Enerji noktasından geçip" + (_maze.gainEnergi * 15) + " enerji puanı kazandınız.",
        $"Maalesef" + _maze.loseEnergy + " Adet Enerji noktasından geçip" + (_maze.loseEnergy * 10) + " enerji puanı kaybettiniz.",
        "Additional Path Information:",
        "--------------------------------"
    };

            // Save existing path information
            foreach (var step in result.Path)
            {
                output.Add(step);
            }

            string filePath = @"C:\Users\mustafa\OneDrive\Masaüstü\MazeResults.txt";
            System.IO.File.AppendAllLines(filePath, output);

            return Ok("Simulation results saved successfully.");
        }

        private bool TraverseMaze(int x, int y, List<string> path, HashSet<(int, int)> visited, ref int resetCount)
        {
            // Log the current position and remaining energy
            path.Add($"({x},{y}) - Energy: {_maze.Energy}");

            // Check if this is the exit point
            if (x == _maze.ExitPoint.x && y == _maze.ExitPoint.y)
            {
                path.Add($"Exit found at ({x},{y}) with {_maze.Energy} energy remaining.");
                return true;
            }

            // Mark current cell as visited
            if (!visited.Add((x, y)))
            {
                return false; // Already visited this cell
            }

            // Define possible moves (up, down, left, right)
            var directions = new List<(int dx, int dy)>
    {
        (1, 0),  // Down
        (-1, 0), // Up
        (0, -1), // Left
        (0, 1)   // Right
    };
            Random random = new Random();
            directions = directions.OrderBy(x => random.Next()).ToList();

            foreach (var (dx, dy) in directions)
            {
                int nextX = x + dx;
                int nextY = y + dy;

                // Check if the next position is walkable and not already visited
                if (_maze.IsWalkable(nextX, nextY) && !visited.Contains((nextX, nextY)))
                {
                    // Update energy for valid moves
                    _maze.UpdateEnergy(_maze.GetNodeValue(nextX, nextY));

                    // Handle reset if energy is depleted
                    if (_maze.Energy <= 0)
                    {
                        if (resetCount >= MaxResets)
                        {
                            path.Add("Maximum resets reached. Unable to solve the maze.");
                            return false;
                        }

                        resetCount++;
                        path.Add($"Energy depleted. Restarting from start point ({_maze.StartPoint.x},{_maze.StartPoint.y}) with {_maze.Energy} energy.");
                        _maze.Reset();
                        visited.Clear(); // Clear visited positions for the reset
                        
                        return TraverseMaze(_maze.StartPoint.x, _maze.StartPoint.y, path, visited, ref resetCount);
                    }

                    // Recursively attempt to traverse the next position
                    if (TraverseMaze(nextX, nextY, path, visited, ref resetCount))
                    {
                        return true;
                    }
                }
            }

            // Mark as dead end and handle backtracking energy deduction

            // Log the current position and energy during backtracking
            _maze.UpdateEnergy(_maze.GetNodeValue(x, y));
                if (_maze.Energy <= 0)
                {
                    if (resetCount >= MaxResets)
                    {
                        path.Add("Energy depleted during backtracking. Unable to solve the maze.");
                        return false;
                    }

                    resetCount++;
                    path.Add($"Energy depleted. Restarting from start point ({_maze.StartPoint.x},{_maze.StartPoint.y}) with {_maze.Energy} energy.");
                    _maze.Reset();
                    visited.Clear(); // Clear visited positions for the reset
                    return TraverseMaze(_maze.StartPoint.x, _maze.StartPoint.y, path, visited, ref resetCount);
                }

            path.Add($"Marking ({x},{y}) as dead end and backtracking - Energy: {_maze.Energy}");

            _maze.MarkAsDeadEnd(x, y); // Mark cell as dead-end
            return false;
        }

        private List<string> SolveMaze()
        {
            List<string> path = new();
            HashSet<(int, int)> visited = new();

            // Start from the initial position
            if (!TraverseMaze(_maze.StartPoint.x, _maze.StartPoint.y, path, visited, ref resetCount))
            {
                path.Add("Unable to solve the maze.");
            }

            return path;
        }

        
    }
}
