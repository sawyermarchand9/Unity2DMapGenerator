using UnityEngine;
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

        public void Generate()
        {
            int current_x = 0;
            int current_y = 0;
            GenerateDungeon(current_x, current_y);

            // for (int i = numberOfRooms - 1; i > 0; i--)
            // {
            //     current_x += width + roomBuffer;
            //     GenerateDungeon(current_x, current_y);
            // }
            // tilemap.SetTile(new Vector3Int(0, 0, 1), null);
        }

        public void GenerateDungeon(int x, int y)
        {
            // Create the root sub-dungeon
            SubDungeon root = new SubDungeon(new RectInt(x, y, width, height));
            root.SplitRecursive(numberOfRooms); // Adjust the split depth as needed
            DrawRooms(root, false);
        }

        void DrawRooms(SubDungeon subDungeon, bool invert)
        {
            if (subDungeon == null)
                return;

            if (subDungeon.IsLeaf())
            {
                for (int x = subDungeon.room.x; x < subDungeon.room.xMax; x++)
                {
                    for (int y = subDungeon.room.y; y < subDungeon.room.yMax; y++)
                    {
                        if(invert)
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), null);
                        }
                        else
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                        }
                    }
                }
            }
            else
            {
                DrawRooms(subDungeon.left, false);
                DrawRooms(subDungeon.right, false);
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
