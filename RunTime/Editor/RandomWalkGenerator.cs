using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapGeneration
{
    public class RandomWalkGenerator
    {

        int numberOfRooms;
        int numberOfSteps;
        public RandomWalkGenerator(int numberOfSteps, int numberOfRooms)
        {
            this.numberOfRooms = numberOfRooms;
            this.numberOfSteps = numberOfSteps;
        }

        public int[,] Generate(int[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    map[i, j] = 1;
                }
            }

            // int numberOfRooms = 4;
            // int numberOfSteps = 500; // Adjust as needed

            // Set the starting point (you can choose any valid position)
            // int startX = width / 2;
            // int startY = height / 2;
            // map[startX, startY] = 0; // Mark the starting point as a walkable area

            // Random random = new Random();

            // for (int step = 0; step < numberOfSteps; step++)
            // {
            //     int direction = random.Next(4); // 0: right, 1: left, 2: up, 3: down

            //     switch (direction)
            //     {
            //         case 0:
            //             startX++;
            //             break;
            //         case 1:
            //             startX--;
            //             break;
            //         case 2:
            //             startY++;
            //             break;
            //         case 3:
            //             startY--;
            //             break;
            //     }

            //     // Ensure the new position is within bounds
            //     startX = Math.Max(0, Math.Min(startX, width - 1));
            //     startY = Math.Max(0, Math.Min(startY, height - 1));

            //     // Mark the current position as a walkable area
            //     map[startX, startY] = 0;
            // }
            for (int i = 0; i < numberOfRooms; i++)
            {
                Random rand = new Random();
                int startX = rand.Next(width);
                int startY = rand.Next(height);

                walk(map, width, height, startX, startY);
                cleanup(map, width, height);
            }

            

            return map;
        }

        private void walk(int[,] map, int width, int height, int startX, int startY)
        {
            // int startX = width / 2;
            // int startY = height / 2;
            map[startX, startY] = 0; // Mark the starting point as a walkable area

            Random random = new Random();

            for (int step = 0; step < numberOfSteps; step++)
            {
                int direction = random.Next(4); // 0: right, 1: left, 2: up, 3: down

                switch (direction)
                {
                    case 0:
                        startX++;
                        break;
                    case 1:
                        startX--;
                        break;
                    case 2:
                        startY++;
                        break;
                    case 3:
                        startY--;
                        break;
                }

                // Ensure the new position is within bounds
                startX = Math.Max(0, Math.Min(startX, width - 1));
                startY = Math.Max(0, Math.Min(startY, height - 1));

                // Mark the current position as a walkable area
                map[startX, startY] = 0;
            }
        }

        private void cleanup(int[,] map, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int surroundingWallCount = GetSurroundingWallCount(i, j, map, width, height);
                    if (surroundingWallCount <= 3)
                    {
                        map[i, j] = 0;
                    }
                }
            }
        }

        int GetSurroundingWallCount(int x, int y, int[,] map, int width, int height)
        {
            int wallCount = 0;

            // Check the 8-connected neighbors
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (i >= 0 && i < width && j >= 0 && j < height) // Bounds check
                    {
                        if (i != x || j != y) // Exclude the center cell
                        {
                            wallCount += map[i, j];
                        }
                    }
                    else
                    {
                        // Handle edge cases (e.g., wrap-around or ignore)
                        // You can customize this part based on your map generation rules.
                    }
                }
            }

            return wallCount;
        }
    }
}