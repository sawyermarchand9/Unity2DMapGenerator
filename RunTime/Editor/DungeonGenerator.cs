using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;

namespace MapGeneration
{
    public class DungeonGenerator
    {
        public TileBase floorTile; // Assign your floor tile here
        public Tilemap tilemap; // Reference to your Tilemap component
        int width { get; set; }
        int height { get; set; }
        int numberOfRooms;

        // int roomBuffer = 15;

        public DungeonGenerator(TileBase floorTile, Tilemap tilemap, int width, int height, int numberOfRooms)
        {
            this.floorTile = floorTile;
            this.tilemap = tilemap;
            this.width = width;
            this.height = height;
            this.numberOfRooms = numberOfRooms;
            
        }

        public void Generate(int current_x, int current_y)
        {
            // GenerateDungeon(current_x, current_y);
        }

        public HashSet<Vector2Int> GenerateDungeon(int x, int y, HashSet<Vector2Int> roomPositions)
        {
            // HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

            SubDungeon root = new SubDungeon(new RectInt(x, y, width, height));
            root.SplitRecursive(numberOfRooms); // Adjust the split depth as needed
            DrawRooms(root, false, roomPositions);
            return roomPositions;
        }
        
        
        void DrawRooms(SubDungeon subDungeon, bool invert, HashSet<Vector2Int> floorPositions)
        {
            if (subDungeon == null)
                return;

            if (subDungeon.IsLeaf())
            {
                for (int x = subDungeon.room.x; x < subDungeon.room.xMax; x++)
                {
                    for (int y = subDungeon.room.y; y < subDungeon.room.yMax; y++)
                    {
                        floorPositions.Add(new Vector2Int(x, y));
                        // if(invert)
                        // {
                        //     // tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        //     floorPositions.Add(new Vector2Int(x, y));
                        // }
                        // else
                        // {
                        //     tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                        // }
                    }
                }
            }
            else
            {
                DrawRooms(subDungeon.left, false, floorPositions);
                DrawRooms(subDungeon.right, false, floorPositions);
            }
        }
    }

    // SubDungeon class for BSP tree structure
    public class SubDungeon
    {
        public RectInt room;
        public SubDungeon left;
        public SubDungeon right;

        public SubDungeon(RectInt room)
        {
            this.room = room;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public void SplitRecursive(int depth)
        {
            if (depth <= 0)
                return;

            // Split the sub-dungeon horizontally or vertically
            bool splitHorizontally = Random.value > 0.5f;
            int splitPosition = splitHorizontally
                ? Random.Range(room.yMin + 5, room.yMax - 5)
                : Random.Range(room.xMin + 5, room.xMax - 5);

            if (splitHorizontally)
            {
                left = new SubDungeon(new RectInt(room.x, room.y, room.width, splitPosition - room.y));
                right = new SubDungeon(new RectInt(room.x, splitPosition, room.width, room.yMax - splitPosition));
            }
            else
            {
                left = new SubDungeon(new RectInt(room.x, room.y, splitPosition - room.x, room.height));
                right = new SubDungeon(new RectInt(splitPosition, room.y, room.xMax - splitPosition, room.height));
            }

            // Recursively split the left and right branches
            left.SplitRecursive(depth - 1);
            right.SplitRecursive(depth - 1);
        }
    }
}
