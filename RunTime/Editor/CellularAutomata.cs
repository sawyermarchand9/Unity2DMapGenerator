using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MapGeneration
{
    public class CellularAutomata
    {
        int height;
        int width;
        public float fillPercentage;
        public int smoothingIterations = 5;

        public CellularAutomata(int height, int width, int smoothingIterations, float fillPercentage)
        {
            this.height = height;
            this.width = width;
            this.smoothingIterations = smoothingIterations;
            this.fillPercentage = fillPercentage;
        }

        public int[,] Generate(int[,] map)
        {
            for (int x = 0; x < width; x++)
            {
            for (int y = 0; y < height; y++)
                {
                    map[x, y] = UnityEngine.Random.value < fillPercentage ? 1 : 0;
                }
            }

            for (int i = 0; i < smoothingIterations; i++)
            {
                SmoothMap(map);
            }
            return map;
        }

        void SmoothMap(int[,] map)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighborWallCount = GetSurroundingWallCount(x, y, map);
                    if (neighborWallCount > 4)
                    {
                        map[x, y] = 1;
                    }
                    else if (neighborWallCount < 4)
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }

        int GetSurroundingWallCount(int x, int y, int[,] map)
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

         void ApplyCellularAutomata(int[,] map)
        {
            // Create a temporary grid to store the updated values
            int[,] tempGrid = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Count the number of neighbors with '-' (walls)
                    int neighborWallCount = 0;

                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        for (int k = x - 1; k <= x + 1; k++)
                        {
                            if (j >= 0 && j < height && k >= 0 && k < width && (j != y || k != x))
                            {
                                if (map[k, j] == 0)
                                {
                                    neighborWallCount++;
                                }
                            }
                        }
                    }

                    // Apply your rules here (e.g., if neighborWallCount > threshold, set tempGrid[x, y] to '-')
                    // For simplicity, let's just copy the value from the original grid
                    tempGrid[x, y] = map[x, y];
                }
            }

            // Update the main grid with the values from the temporary grid
            map = tempGrid;
        }
    }

}