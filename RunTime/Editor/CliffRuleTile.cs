public class CliffRuleTile : RuleTile<CliffRuleTile.Neighbor> {
    public Sprite topSprite;
    public Sprite faceSprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        tileData.sprite = topSprite;
        tileData.colliderType = Tile.ColliderType.Sprite;

        // Add the cliff face as a GameObject or use a custom renderer
        tileData.gameObject = CreateCliffFaceObject(faceSprite);
    }

    private GameObject CreateCliffFaceObject(Sprite faceSprite) {
        var go = new GameObject("CliffFace");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = faceSprite;
        sr.sortingOrder = -1; // Render behind top
        go.transform.localPosition = new Vector3(0, -1, 0); // Offset down
        return go;
    }

    public class Neighbor : RuleTile.TilingRuleOutput.Neighbor { }
}
