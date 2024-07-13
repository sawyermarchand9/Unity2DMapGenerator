using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MapGeneration
{
    public class CellularAutomata
    {
        int height;

        int width;

        int density;
        public CellularAutomata(int height, int width, int density)
        {
            this.height = height;
            this.width = width;
            this.density = density;
        }

        public int[,] Generate(int[,] map)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int rand = UnityEngine.Random.Range(1, 101);
                    map[x, y] = rand > density ? 0 : 1;
                }
            }
            ApplyCellularAutomata(map);
            return map;
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