using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapGeneration
{
    public class GeneratorBase
    {
        protected int height;
        protected int width;
        

        public GeneratorBase(int width, int height)
        {
            this.width = width;
            this.height = height;
            // this.map = GenerateArray(true);
        }

        // protected int[,] GenerateArray(bool empty)
        // {
        //     int[,] temp = new int[width, height];
        //     for(int x = 0; x < width; x++)
        //     {
        //         for(int y = 0; y < height; y++)
        //         {
        //             temp[x,y] = (empty)?0:1;
        //         }
        //     }
        //     return temp;
        // }
    }
}