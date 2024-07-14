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

    }

}