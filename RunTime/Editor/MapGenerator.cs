using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


public class MapGenerator : EditorWindow
{
    int height = 10;
    int width = 10;

    float smooth = 10;
    float seed;
    float modifier = 0.1f;
    int smoothingIterations = 0;

    // Random Walk
    int randomWalkIterations = 5;
    int numberOfRooms = 60;
    int numberOfSteps = 50;
    int numberOfHalls = 2;
    float randomWalkModifier = 0.8f;
    bool startRandomlyEachIteration = false;

    GameObject parentObject;
    TileBase groundTile;
    TileBase wallTile;
    Tilemap groundTileMap;
    Tilemap wallTileMap;
    GameObject gameObject;
    int numberOfGameObjects = 10;
    int selectedOption = 0;


    string[] algorithms = new string[] { "Perlin Noise", "Cellular Automata", "Random Walk" };
    int[,] map;
    bool invert = false;
    float fillPercentage;

    Coordinate[,] gameObjectLocations;

    public class Coordinate
    {
        public int x { get; set; }
        public int y { get; set; }
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }


    private bool includeGameObject = false;

    [MenuItem("Volcanic Garage/Tools/Map Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MapGenerator));
    }

    private void OnGUI()
    {
        GUILayout.Space(10.0f);
        GUILayout.Label("Algorithms", EditorStyles.boldLabel);
        GUILayout.Space(3.0f);

        EditorGUILayout.LabelField("Select an Algorithm:");
        selectedOption = EditorGUILayout.Popup(selectedOption, algorithms);

        GUILayout.Space(10.0f);
        GUILayout.Label("Objects", EditorStyles.boldLabel);
        GUILayout.Space(3.0f);


        groundTile = EditorGUILayout.ObjectField("Rules Tile", groundTile, typeof(TileBase), false) as TileBase;
        groundTileMap = EditorGUILayout.ObjectField("Tile Map", groundTileMap, typeof(Tilemap), true) as Tilemap;
        includeGameObject = EditorGUILayout.Toggle("Include Object (Beta)", includeGameObject);
        if (includeGameObject)
        {
            numberOfGameObjects = EditorGUILayout.IntField("Number of Game Objects", numberOfGameObjects);
            int prefabCount = Mathf.Max(1, EditorGUILayout.IntField("Number of Prefab Types", gameObjectsPrefabs.Count));
            while (gameObjectsPrefabs.Count < prefabCount)
                gameObjectsPrefabs.Add(null);
            while (gameObjectsPrefabs.Count > prefabCount)
                gameObjectsPrefabs.RemoveAt(gameObjectsPrefabs.Count - 1);

            for (int i = 0; i < gameObjectsPrefabs.Count; i++)
            {
                gameObjectsPrefabs[i] = EditorGUILayout.ObjectField($"Game Object {i + 1}", gameObjectsPrefabs[i], typeof(GameObject), true) as GameObject;
            }
        }

        EditorGUILayout.Separator();
        GUILayout.Space(15.0f);

        GUILayout.Label("Dimensions", EditorStyles.boldLabel);
        GUILayout.Space(3.0f);
        height = EditorGUILayout.IntField("Height", height);
        width = EditorGUILayout.IntField("Width", width);
        GUILayout.Space(15.0f);

        if (selectedOption == 0)
        {
            GUILayout.Label("Perlin Params", EditorStyles.boldLabel);
            GUILayout.Space(3.0f);
            smooth = EditorGUILayout.FloatField("Smoothness", smooth);
            seed = EditorGUILayout.FloatField("Seed", seed);
            modifier = EditorGUILayout.FloatField("Modifier", modifier);

            invert = EditorGUILayout.Toggle("Invert Tilemap", invert);
        }
        if (selectedOption == 1)
        {
            GUILayout.Label("CA Params", EditorStyles.boldLabel);
            GUILayout.Space(3.0f);
            fillPercentage = EditorGUILayout.Slider("Fill Percent", fillPercentage, 0f, 1f);
            GUILayout.Space(20.0f);
            smoothingIterations = EditorGUILayout.IntField("Smoothness", smoothingIterations);
        }
        else if (selectedOption == 2 || selectedOption == 3)
        {
            GUILayout.Label("CA Params", EditorStyles.boldLabel);
            GUILayout.Space(3.0f);
            wallTileMap = EditorGUILayout.ObjectField("Tile Map", wallTileMap, typeof(Tilemap), true) as Tilemap;
            GUILayout.Space(3.0f);
            wallTile = EditorGUILayout.ObjectField("Wall Rule Tile", wallTile, typeof(TileBase), false) as TileBase;
            GUILayout.Space(3.0f);
            numberOfSteps = EditorGUILayout.IntField("Number of Steps", numberOfSteps);
            GUILayout.Space(3.0f);
            numberOfHalls = EditorGUILayout.IntField("Number of Rooms", numberOfHalls);
            GUILayout.Space(3.0f);
            numberOfRooms = EditorGUILayout.IntField("Room Size", numberOfRooms);
            GUILayout.Space(3.0f);
            randomWalkIterations = EditorGUILayout.IntField("Iterations", randomWalkIterations);
            GUILayout.Space(3.0f);
            randomWalkModifier = EditorGUILayout.Slider("Modifier", randomWalkModifier, 0f, 1f);
            GUILayout.Space(3.0f);
            startRandomlyEachIteration = EditorGUILayout.Toggle("Random Iteration", startRandomlyEachIteration);

        }


        GUILayout.Space(15.0f);


        if (GUILayout.Button("Generate Map"))
        {
            DestroyImmediate(parentObject);
            GenerateMap();
        }


        if (GUILayout.Button("Regenerate"))
        {
            if (parentObject != null)
            {
                DestroyImmediate(parentObject);
            }

            ResetTileMap();
            GenerateMap();
        }

        if (GUILayout.Button("Reset Tile Map"))
        {
            if (parentObject != null)
            {
                DestroyImmediate(parentObject);
            }

            ResetTileMap();
        }

        // if(GUILayout.Button("Check Dimensions"))
        // {
        //     if(groundTileMap != null)
        //     {

        //     }
        // }
    }

    private void ResetTileMap()
    {
        groundTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();
    }

    private void GenerateMap()
    {
        Debug.Log("Generating the Map . . .");
        map = GenerateArray(width, height, true);
        map = TerrenGeneration(map);
        if (selectedOption != 3 && selectedOption != 2)
        {
            RenderMap(map);
        }
        Selection.activeGameObject = groundTileMap.gameObject;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }
        Debug.Log("Done Generating Map . . .");
    }

    public int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (empty) ? 0 : 1;
            }
        }
        return map;
    }

    public void addGameObject(int[,] map, int x, int y)
    {
        if (parentObject == null)
        {
            parentObject ??= GameObject.Find("GeneratedObject");
            if (parentObject == null)
            {
                parentObject = new GameObject("GeneratedObject");
            }
        }
        if (y < height - 1 && y > 1 && x > 1 && x < width - 1)
        {
            float mapOX = groundTileMap.transform.position.x;
            float mapOY = groundTileMap.transform.position.y;
            if (map[x, y + 1] == 0 && map[x, y] == 1)
            {
                float newX = x + mapOX + 0.5f;
                float newY = y + mapOY + 1.5f;
                Vector3 position = new Vector3(newX, newY, 0);
                bool alreadyPlaced = gameObjects.Any(go => go.transform.position == position);
                if (!alreadyPlaced && gameObject != null)
                {
                    Debug.Log($"Placing object at {position}");
                    GameObject go = Instantiate(gameObject, position, Quaternion.identity);
                    gameObjects.Add(go);
                    go.transform.parent = parentObject.transform;
                }
            }
        }
    }

    public int[,] TerrenGeneration(int[,] map)
    {
        if (selectedOption == 0)
        {
            PerlinGenerator generator = new PerlinGenerator(height, width, seed, smooth, modifier, invert);
            map = generator.Generate(map);
        }
        else if (selectedOption == 1)
        {
            CellularAutomata generator = new CellularAutomata(height, width, smoothingIterations, fillPercentage);
            generator.Generate(map);
        }
        else if (selectedOption == 2)
        {
            RandomWalkGenerator generator = new RandomWalkGenerator(numberOfSteps, numberOfHalls, numberOfRooms, randomWalkIterations, randomWalkModifier, startRandomlyEachIteration);
            HashSet<Vector2Int> rooms = generator.Generate(groundTileMap, wallTileMap, groundTile, wallTile);

            RoomCenterGizmos roomCenterGizmos = FindObjectOfType<RoomCenterGizmos>();
            if (roomCenterGizmos != null)
            {
                roomCenterGizmos.generator = generator;
                roomCenterGizmos.targetTilemap = groundTileMap;
            }
            var rand = new System.Random();
            if (gameObjectsPrefabs.Count > 0 && gameObject == null)
            {
                // Pick a random prefab from the list
                gameObject = gameObjectsPrefabs[rand.Next(gameObjectsPrefabs.Count)];
            }
            if (includeGameObject && gameObject != null)
            {
                int numberOfObjects = numberOfGameObjects;
                int objectsPlaced = 0;
                
                if (parentObject == null)
                {
                    parentObject = new GameObject("GeneratedObject");
                }

                // Collect all valid tile positions from the groundTileMap
                List<Vector3Int> validPositions = new List<Vector3Int>();
                foreach (var pos in groundTileMap.cellBounds.allPositionsWithin)
                {
                    if (groundTileMap.HasTile(pos))
                    {
                        validPositions.Add(pos);
                    }
                }

                // Shuffle the positions for randomness
                validPositions = validPositions.OrderBy(p => rand.Next()).ToList();

                float minDistance = 2.0f; // Minimum distance between objects

                foreach (var cellPos in validPositions)
                {
                    if (objectsPlaced >= numberOfObjects)
                        break;
                    gameObject = gameObjectsPrefabs[rand.Next(gameObjectsPrefabs.Count)];
                    // Add random offset within a small range around the cell
                    int x = cellPos.x + rand.Next(-3, 4); // -3 to 3
                    int y = cellPos.y + rand.Next(-3, 4);
                    Vector3Int randomCell = new Vector3Int(x, y, 0);

                    if (groundTileMap.HasTile(randomCell))
                    {
                        Vector3 worldPos = groundTileMap.CellToWorld(randomCell) + new Vector3(0.5f, 0.5f, 0);

                        // Check if already placed at this location or too close to another object
                        bool tooClose = parentObject.transform.Cast<Transform>()
                            .Any(t => Vector3.Distance(t.position, worldPos) < minDistance);

                        if (!tooClose)
                        {
                            Debug.Log($"Placing object at {worldPos}");
                            GameObject go = PrefabUtility.InstantiatePrefab(gameObject) as GameObject;
                            if (go != null)
                            {
                                go.transform.position = worldPos;
                                go.transform.parent = parentObject.transform;
                                objectsPlaced++;
                            }
                        }
                    }
                }
            }

        }
        // else if (selectedOption == 3)
        // {
        //     DungeonGenerator generator = new DungeonGenerator(groundTile, groundTileMap, width, height, numberOfRooms);
        //     generator.Generate();
        // }
        return map;
    }

    public List<GameObject> gameObjects = new List<GameObject>(); // Move this to class level
    public List<GameObject> gameObjectsPrefabs = new List<GameObject>();

    public void RenderMap(int[,] map)
    {
        var random = new System.Random();
        int maxObjects = 10; // Set a max number of objects to spawn
        int objectsPlaced = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {

                    groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTile);

                    // Place objects only if not on edge and not already placed
                    if (objectsPlaced < maxObjects && x > 1 && x < width - 2 && y > 1 && y < height - 2)
                    {
                        // Random chance to place
                        if (random.NextDouble() < 0.02) // 2% chance
                        {
                            // Check if already placed at this location
                            Vector3 position = new Vector3(x + groundTileMap.transform.position.x + 0.5f, y + groundTileMap.transform.position.y + 1.5f, 0);
                            bool alreadyPlaced = gameObjects.Any(go => go.transform.position == position);

                            if (!alreadyPlaced && gameObject != null)
                            {
                                GameObject go = Instantiate(gameObject, position, Quaternion.identity);
                                gameObjects.Add(go);
                                if (parentObject == null)
                                    parentObject = new GameObject("GeneratedObject");
                                go.transform.parent = parentObject.transform;
                                objectsPlaced++;
                            }
                        }
                    }
                }
                if (y == 0 || x == width - 1 || x == 0)
                {
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTile);
                }
            }
        }
    }
}
