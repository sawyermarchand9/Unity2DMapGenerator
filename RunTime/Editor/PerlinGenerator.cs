using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MapGeneration
{
    public class PerlinGenerator
    {
        private int height;
        private int width;
        private float seed;
        private float modifier;
        private float smoothness;
        private bool invert;

        public PerlinGenerator(int height, int width, float seed, float smoothness, float modifier, bool invert)
        {
            this.height = height;
            this.width = width;
            this.seed = seed;
            this.smoothness = smoothness;
            this.modifier = modifier;
            this.invert = invert;
        }

        public int[,] Generate(int[,] map)
        {
            int perlinHeight;
            for (int x = 0; x < width; x++)
            {
                perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / smoothness, seed) * height / 2);
                perlinHeight += height / 2;
                for (int y = 0; y < perlinHeight; y++)
                {
                    int caveValue = Mathf.RoundToInt(Mathf.PerlinNoise((x * modifier) + seed, (y * modifier) + seed));
                    if (invert)
                    {
                        map[x, y] = (caveValue == 1) ? 0 : 1;
                    }
                    else
                    {
                        map[x, y] = caveValue;
                    }

                }
            }
            return map;
        }
    }
}
