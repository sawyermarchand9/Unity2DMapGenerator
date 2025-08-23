using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;

namespace MapGeneration
{
    public class RandomWalkGenerator
    {

        int numberOfRooms;
        int numberOfSteps;
        int iterations;
        float modifier;
        bool startRandomlyEachIteration;
        int numberOfHalls;
        HallMonitor hallMonitor;
        DungeonGenerator generator;

        public List<Vector2> roomCenters = new List<Vector2>();
        public HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
        public RandomWalkGenerator(int numberOfSteps, int numberOfHalls, int numberOfRooms, int iterations, float modifier, bool startRandomlyEachIteration)
        {
            this.numberOfRooms = numberOfRooms;
            this.numberOfSteps = numberOfSteps;
            this.iterations = iterations;
            this.modifier = modifier;
            this.startRandomlyEachIteration = startRandomlyEachIteration;
            this.numberOfHalls = numberOfHalls;
            this.hallMonitor = new HallMonitor(iterations);
        }

        public HashSet<Vector2Int> Generate(Tilemap tilemap, Tilemap wallTileMap, TileBase tile, TileBase wallTile)
        {
            Vector2Int startPosition = new Vector2Int((int)tilemap.transform.position.x, (int)tilemap.transform.position.y);
            // HashSet<Vector2Int> floorPositions = RunRandomWalk((int)tilemap.transform.position.x, (int)tilemap.transform.position.y);
            HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
            HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();
            generator = new DungeonGenerator(tile, tilemap, 20, 20, 10);

            List<List<Vector2Int>> halls = CreateHalls(floorPositions, potentialRoomPositions, startPosition, numberOfHalls);

            for (int i = 0; i < halls.Count; i++)
            {
                halls[i] = IncreaseHallSizeByOne(halls[i]);
                floorPositions.UnionWith(halls[i]);
            }

            rooms = CreateRooms(potentialRoomPositions);

            floorPositions.UnionWith(rooms);



            foreach (var position in floorPositions)
            {
                var tilePosition = tilemap.WorldToCell((Vector3Int)position);
                tilemap.SetTile(tilePosition, tile);
            }
            CreateWalls(floorPositions, wallTileMap, wallTile);

            return floorPositions;
        }

        public List<Vector2Int> IncreaseHallSizeByOne(List<Vector2Int> halls)
        {
            List<Vector2Int> newHalls = new List<Vector2Int>();
            Vector2Int previousDirection = Vector2Int.zero;

            for (int i = 1; i < halls.Count; i++)
            {
                Vector2Int directionFromCell = halls[i] - halls[i - 1];

                if (previousDirection != Vector2Int.zero &&
                    directionFromCell != previousDirection)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            newHalls.Add(halls[i - 1] + new Vector2Int(x, y));
                        }
                    }
                    previousDirection = directionFromCell;
                }
                else
                {
                    Vector2Int newHallTileOffset = GetDirection90From(directionFromCell);
                    newHalls.Add(halls[i - 1]);
                    newHalls.Add(halls[i - 1] + newHallTileOffset);
                }
            }

            return newHalls;
        }

        public Vector2Int GetDirection90From(Vector2Int direction)
        {
            if (direction == Vector2Int.up)
            {
                return Vector2Int.right;
            }
            else if (direction == Vector2Int.right)
            {
                return Vector2Int.down;
            }
            else if (direction == Vector2Int.down)
            {
                return Vector2Int.left;
            }
            else if (direction == Vector2Int.left)
            {
                return Vector2Int.up;
            }
            return Vector2Int.zero;
        }

        public HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
        {
            HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
            int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * modifier);
            List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();

            foreach (var roomPosition in roomsToCreate)
            {
                var roomFloor = RunRandomWalk(roomPosition);
                // Calculate center of the room
                if (roomFloor.Count > 0)
                {
                    int minX = roomFloor.Min(pos => pos.x);
                    int maxX = roomFloor.Max(pos => pos.x);
                    int minY = roomFloor.Min(pos => pos.y);
                    int maxY = roomFloor.Max(pos => pos.y);

                    Vector2 center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

                    // Example: Log or use the center
                    roomCenters.Add(center);
                    Debug.Log($"Room center at: {center}");
                    // You can also store these centers in a list if needed
                }
                generator.GenerateDungeon(roomPosition.x - 10, roomPosition.y - 10, roomPositions);

                roomPositions.UnionWith(roomFloor);
            }

            return roomPositions;
        }

        public HashSet<Vector2Int> RunRandomWalk(Vector2Int startPosition)
        {
            var currentPosition = startPosition;
            HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();


            for (int i = 0; i < numberOfRooms; i++)
            {
                var path = Walk(currentPosition, numberOfSteps);
                floorPositions.UnionWith(path);
                if (startRandomlyEachIteration)
                {
                    currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
                }
            }



            /** TODO: 
                implement cleanup method or option to remove single

                possible implementation:
                    take center postion x + and - number of steps
                    take center postion x + and - number of steps   
            */




            return floorPositions;
        }

        public HashSet<Vector2Int> Walk(Vector2Int startpos, int walkLength)
        {
            HashSet<Vector2Int> path = new HashSet<Vector2Int>();

            path.Add(startpos);

            var previousPosition = startpos;

            for (int i = 0; i < walkLength; i++)
            {
                var newPosition = previousPosition + Direction2D.getRandomCardinalDirection();

                path.Add(newPosition);

                previousPosition = newPosition;
            }
            return path;
        }

        private void CreateWalls(HashSet<Vector2Int> floorPositions, Tilemap tilemap, TileBase wallTile)
        {
            // TODO use this to border to create filler for floor positions
            var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.CardinalDirections);
            foreach (var position in basicWallPositions)
            {
                var tilePosition = tilemap.WorldToCell((Vector3Int)position);
                tilemap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, -1), wallTile);
            }
        }

        private HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
        {
            HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
            foreach (var position in floorPositions)
            {
                foreach (var direction in directionsList)
                {
                    var neighborPosition = position + direction;
                    if (floorPositions.Contains(neighborPosition) == false)
                    {
                        wallPositions.Add(neighborPosition);
                    }
                }
            }
            return wallPositions;
        }

        public List<List<Vector2Int>> CreateHalls(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions, Vector2Int startPosition, int numberOfHalls)
        {
            List<List<Vector2Int>> halls = new List<List<Vector2Int>>();
            var currentPosition = startPosition;
            potentialRoomPositions.Add(currentPosition);

            for (int i = 0; i < numberOfHalls; i++)
            {
                var path = CreateHall(currentPosition, iterations);
                halls.Add(path);
                currentPosition = path[path.Count - 1];
                floorPositions.UnionWith(path);
                potentialRoomPositions.Add(currentPosition);
            }
            return halls;
        }

        public List<Vector2Int> CreateHall(Vector2Int startPosition, int hallLength)
        {
            List<Vector2Int> halls = new List<Vector2Int>();
            var direction = Direction2D.getRandomCardinalDirection();
            var currentPosition = startPosition;
            halls.Add(currentPosition);

            for (int i = 0; i < hallLength; i++)
            {
                currentPosition += direction;
                halls.Add(currentPosition);
            }
            return halls;
        }

    }

    public static class Direction2D
    {
        public static List<Vector2Int> CardinalDirections = new List<Vector2Int>
        {
            new Vector2Int(0,1), // up
            new Vector2Int(1,0), // right
            new Vector2Int(0,-1), // down
            new Vector2Int(-1,0) // left
        };

        public static Vector2Int getRandomCardinalDirection()
        {
            return CardinalDirections[Random.Range(0, CardinalDirections.Count)];
        }
    }

}
