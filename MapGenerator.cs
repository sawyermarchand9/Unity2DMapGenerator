using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
public class MapGenerator : EditorWindow
{
    int height= 10;
    int width = 10;
    
    float smooth = 10;
    float seed;
    float modifier = 0.1f;
    GameObject parentObject;
    TileBase groundTile;
    Tilemap groundTileMap;
    GameObject gameObject;

    int[,] map;
    Coordinate[,] gameObjectLocations;

    public class Coordinate{
        public int x {get; set;}
        public int y {get; set;}
        Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private bool openCaves;

    [MenuItem("Tools/Map Generator")]
    public static void ShowWindow(){
        GetWindow(typeof(MapGenerator));
        
    }

    private void OnGUI(){
        GUILayout.Space(10.0f);
        GUILayout.Label("Objects", EditorStyles.boldLabel);
        GUILayout.Space(3.0f);

        groundTile = EditorGUILayout.ObjectField("Rules Tile", groundTile, typeof(TileBase), false) as TileBase;
        groundTileMap = EditorGUILayout.ObjectField("Tile Map", groundTileMap, typeof(Tilemap), true) as Tilemap;
        gameObject = EditorGUILayout.ObjectField("Game object", gameObject, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Separator();
        GUILayout.Space(15.0f);
        
        GUILayout.Label("Dimensions", EditorStyles.boldLabel);
        GUILayout.Space(3.0f);
        height = EditorGUILayout.IntField("Height", height);
        width = EditorGUILayout.IntField("Width", width);
        GUILayout.Space(15.0f);
        GUILayout.Label("Perlin Params", EditorStyles.boldLabel);
        GUILayout.Space(3.0f);
        smooth = EditorGUILayout.FloatField("Smoothness", smooth);
        seed = EditorGUILayout.FloatField("Seed", seed);
        modifier = EditorGUILayout.FloatField("Modifier", modifier);

        openCaves = EditorGUILayout.Toggle("Open Caves", openCaves);
        
        GUILayout.Space(15.0f);
 

        if(GUILayout.Button("Generate Map"))
        {
            Destroy(parentObject);
            GenerateMap();
        }
       

        if(GUILayout.Button("Regenerate"))
        {
            Destroy(parentObject);
            ResetTileMap();
            GenerateMap();
        }

        if(GUILayout.Button("Reset Tile Map"))
        {
            Destroy(parentObject);
            ResetTileMap();
        }

        if(GUILayout.Button("Check Dimensions"))
        {
            if(groundTileMap != null)
            {
                
            }
        }
    }

    private void ResetTileMap()
    {
        groundTileMap.ClearAllTiles();
    }

    private void GenerateMap(){
        Debug.Log("Generating the Map . . .");
        map = GenerateArray(width, height, true);
        map = TerrenGeneration(map);
        RenderMap(map);
    }

    public int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                map[x,y] = (empty)?0:1;
                
            }
        }
        return map;
    }

    public void addGameObject(int[,] map, int x, int y){
        parentObject = GameObject.Find("GeneratedObject");
        System.Collections.Generic.List<GameObject> gameObjects = new System.Collections.Generic.List<GameObject>(); 
        if(y < height-1 && y > 1 && x > 1 && x < width -1)
        {
            // get position relitive to tile map
            float mapOX = groundTileMap.transform.position.x;
            float mapOY = groundTileMap.transform.position.y;
            // subtract the offset
            if(map[x,y+1]==0 && map[x,y]==1)
            {
                float newX = (x+mapOX)+.5f;
                float newY = (y+mapOY)+1.5f;
                if(parentObject == null)
                {
                    parentObject = new GameObject("GeneratedObject");
                }
                
                if(gameObjects.Count > 0)
                {
                    if(gameObjects[gameObjects.Count].transform.position.x+50 > newX)
                    {
                        GameObject go = Instantiate(gameObject, new Vector3(newX, newY, 0), Quaternion.identity);
                        gameObjects.Add(go);
                        go.transform.parent = parentObject.transform;
                    }
                    
                }else if(gameObjects.Count == 0)
                {
                    GameObject go = Instantiate(gameObject, new Vector3(newX, newY, 0), Quaternion.identity);
                    gameObjects.Add(go);
                    go.transform.parent = parentObject.transform;
                }
                

                
                
            }
            
        }
    }

    public int[,] TerrenGeneration(int[,] map)
    {   
        int perlinHeight;
        for(int x = 0; x < width; x++)
        {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x/smooth, seed)*height/2);
            perlinHeight += height/2;
            for(int y = 0; y < perlinHeight; y++)
            {
                int caveValue = Mathf.RoundToInt(Mathf.PerlinNoise((x*modifier)+seed, (y*modifier)+seed));
                if(openCaves)
                {
                    map[x,y] = (caveValue == 1)? 0 : 1;
                }
                else
                {
                    map[x,y] = caveValue;
                }
                
            }
        }
        return map;
    }

    public void RenderMap(int[,] map)
    {
        
        var random = new System.Random();
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(map[x,y] == 1)
                {
                    
                    groundTileMap.SetTile(new Vector3Int(x,y,0), groundTile);
                }
                if(y == 0 || x == width-1 || x == 0)
                {
                    groundTileMap.SetTile(new Vector3Int(x,y,0), groundTile);
                }
                int num = random.Next(1, 20);
                if(num == 6 )
                {
                    addGameObject(map, x, y);
                }
            }
        }
    }
}
