using UnityEngine;
using UnityEditor;
using System.IO;
using SysEnum = System.Enum;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor.AssetImporters; // For Sprite slicing
using System.Linq;
using System.Text.RegularExpressions;

public class RuleTileGenerator : EditorWindow
{
    private Texture2D spriteSheet;
    private string outputPath = "Assets/GeneratedTiles/";

    private int cellSize = 16;

    [MenuItem("Volcanic Garage/Tools/RuleTile Generator")]
    public static void ShowWindow()
    {
        GetWindow<RuleTileGenerator>("RuleTile Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generate RuleTiles from Sprite Sheet", EditorStyles.boldLabel);
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet", spriteSheet, typeof(Texture2D), false);
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button("Generate RuleTiles"))
        {
            string path = AssetDatabase.GetAssetPath(spriteSheet);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple; // Important for sprite sheets
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            // Optional: Auto-slice using grid
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true;

            // Example: 16x16 grid slicing
            importer.spritePixelsPerUnit = 16;
            importer.spritesheet = AutoSliceGrid(spriteSheet, 16, 16);

            importer.SaveAndReimport();

            Debug.Log($"Applied pixel art settings to sprite sheet: {spriteSheet.name}");

            GenerateRuleTile();
        }
    }

    void GenerateRuleTile()
    {
        if (spriteSheet == null) return;

        string path = AssetDatabase.GetAssetPath(spriteSheet);
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

        // Collect all sprites from the sheet
        List<Sprite> spritesTemp = new List<Sprite>();
        for (int i = 0; i < assets.Length; i++)
        {
            object asset = assets[i];
            if (asset is Sprite sprite)
            {
                spritesTemp.Add(sprite);
                Debug.Log("Found sprite: " + sprite.name);
            }
        }

        List<Sprite> sprites = SortByGridPosition(spritesTemp);

        // if (sprites.Count != 9)
        // {
        //     Debug.LogError("Expected 9 sprites in the sheet (3x3 grid). Found: " + sprites.Count);
        //     return;
        // }

        var ruleTile = ScriptableObject.CreateInstance<RuleTile>();
        ruleTile.m_DefaultSprite = sprites[4]; // Center sprite as default
        ruleTile.m_DefaultColliderType = Tile.ColliderType.Sprite;
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>();

        // 3x3 grid: indices
        // [0][1][2]
        // [3][4][5]
        // [6][7][8]
        // Unity's neighbor order: [TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight]

        int[][] neighborConfigs;

        if (sprites.Count == 9) { neighborConfigs = RuleTileLogic.ThreeByThreeGrid; }
        else if (sprites.Count == 13) { neighborConfigs = RuleTileLogic.FourByThree; }
        else
        {
            Debug.LogError("Unsupported number of sprites for predefined configurations. Found: " + sprites.Count);
            return;
        }


        for (int i = 0; i < sprites.Count; i++)
        {
            var rule = new RuleTile.TilingRule();
            rule.m_Sprites = new Sprite[] { sprites[i] };
            rule.m_Neighbors = new List<int>(neighborConfigs[i]);
            ruleTile.m_TilingRules.Add(rule);
        }

        string tilePath = Path.Combine(outputPath, spriteSheet.name + "_RuleTile.asset");
        AssetDatabase.CreateAsset(ruleTile, tilePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Single RuleTile generated from sprite sheet!");
    }

    private static SpriteMetaData[] AutoSliceGrid(Texture2D texture, int cellWidth, int cellHeight)
    {
        int columns = texture.width / cellWidth;
        int rows = texture.height / cellHeight;
        List<SpriteMetaData> slices = new List<SpriteMetaData>();

        for (int y = 0; y < rows; y++) // Top to bottom
        {
            for (int x = 0; x < columns; x++)
            {
                int flippedY = rows - 1 - y;
                Rect rect = new Rect(x * cellWidth, flippedY * cellHeight, cellWidth, cellHeight);

                // Check if tile is empty
                if (IsTileEmpty(texture, rect))
                    continue;

                SpriteMetaData meta = new SpriteMetaData
                {
                    rect = rect,
                    name = $"sprite_{x}_{y}",
                    alignment = (int)SpriteAlignment.Center
                };

                slices.Add(meta);
            }
        }

        return slices.ToArray();
    }

    private static bool IsTileEmpty(Texture2D texture, Rect rect)
    {
        if (!texture.isReadable)
        {
            Debug.LogWarning("Texture is not readable.");
            return true; // Treat as empty or skip
        }

        Color[] pixels;
        try
        {
            pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        }
        catch (UnityException e)
        {
            Debug.LogError($"Error getting pixels from texture: {e.Message}");
            return true; // Treat as empty if we can't read the pixels
        }

        foreach (Color pixel in pixels)
        {
            if (pixel.a > 0.01f) // Any visible pixel
                return false;
        }

        return true;
    }

    public static List<Sprite> SortByGridPosition(List<Sprite> sprites)
    {
        var regex = new Regex(@"sprite_(\d+)_(\d+)", RegexOptions.Compiled);

        return sprites
            .Select(sprite =>
            {
                var match = regex.Match(sprite.name);
                int x = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                int y = match.Success ? int.Parse(match.Groups[2].Value) : 0;
                return new { sprite, x, y };
            })
            .OrderBy(s => s.x) // Column-major: sort by x first
            .ThenBy(s => s.y)  // Then by y
            .Select(s => s.sprite)
            .ToList();
    }

    private static class RuleTileLogic
    {
        public static int[] TOP = new int[] { 0, 2, 0, 1, 1, 1, 1, 1 };
        public static int[] TOP_LEFT = new int[] { 2, 2, 0, 2, 1, 0, 1, 1 };
        public static int[] TOP_RIGHT = new int[] { 0, 2, 2, 1, 2, 1, 1, 0 };
        public static int[] BOTTOM = new int[] { 1, 1, 1, 1, 1, 0, 2, 0 };
        public static int[] BOTTOM_LEFT = new int[] { 0, 1, 1, 2, 1, 2, 2, 0 };
        public static int[] BOTTOM_RIGHT = new int[] { 1, 1, 0, 1, 2, 0, 2, 2 };
        public static int[] LEFT = new int[] { 0, 1, 1, 2, 1, 0, 1, 1 };
        public static int[] RIGHT = new int[] { 1, 1, 0, 1, 2, 1, 1, 0 };
        public static int[] CENTER = new int[] { 1, 1, 1, 1, 1, 1, 1, 1 };
        public static int[] TOP_LEFT_CORNER = new int[] { 1, 1, 1, 1, 1, 1, 1, 2 };
        public static int[] TOP_RIGHT_CORNER = new int[] { 1, 1, 1, 1, 1, 2, 1, 1 };
        public static int[] BOTTOM_LEFT_CORNER = new int[] { 1, 1, 2, 1, 1, 1, 1, 1 };
        public static int[] BOTTOM_RIGHT_CORNER = new int[] { 2, 1, 1, 1, 1, 1, 1, 1 };

        public static int[][] ThreeByThreeGrid = new int[][]
        {
            TOP_LEFT, LEFT, BOTTOM_LEFT,
            TOP, CENTER, BOTTOM,
            TOP_RIGHT, RIGHT, BOTTOM_RIGHT
        };

        public static int[][] FourByThree = new int[][]
        {
            TOP_LEFT, LEFT, BOTTOM_LEFT,
            TOP, CENTER, BOTTOM,
            TOP_RIGHT, RIGHT, BOTTOM_RIGHT,
            TOP_LEFT_CORNER, BOTTOM_LEFT_CORNER,
            TOP_RIGHT_CORNER, BOTTOM_RIGHT_CORNER
        };
        
    }
}
